using Event_Creator.models;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.MiddleWare
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class SecurityMiddleWare
    {
        private readonly RequestDelegate _next;

        public SecurityMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext , ApplicationContext dbContext , IUserService userService)
        {
            
            
            if (httpContext.Request.Headers.ContainsKey("Authorization")== false) {
                await _next(httpContext);
            }
            else
            {
                var authorizationHeader = httpContext.Request.Headers.Single(x => x.Key.Equals("Authorization"));
                var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(stream);
                var tokenS = jsonToken as JwtSecurityToken;
                var jti = tokenS.Claims.First(claim => claim.Type == "jti").Value;
                /// if the jwt access token was revoked! 
                if (await dbContext.jwtBlackLists.SingleOrDefaultAsync(x => x.jwtToken.Equals(jti)) != null) {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Revoked Token!");
                    return;
                }
                RefreshToken refresh = await dbContext.refreshTokens.SingleOrDefaultAsync(x => x.JwtTokenId.Equals(jti.ToString()));
                if(refresh.UserAgent != httpContext.Request.Headers.FirstOrDefault(x => x.Key.Contains("User-Agent")).ToString()){
                    httpContext.Response.StatusCode = 403;
                    await httpContext.Response.WriteAsync("malicous client!");
                    return;
                }
                string ip = httpContext.Connection.RemoteIpAddress.ToString();
                if (ip != refresh.ipAddress)
                {
                    refresh.ipAddress = ip;
                    dbContext.refreshTokens.Update(refresh);
                    await dbContext.SaveChangesAsync();
                    Task.Run(() => {
                        dbContext.Entry(refresh).Reference(x => x.user).Load();
                        TextPart text = new TextPart("plain")
                        {
                            Text = $" وارد حساب کاربری شما شده است در صورتی که فرد وارد شده شما نیستید لطفااتمام همه نشست ها را بزنید و پسورد خود را عوض کرده و دوباره وارد شوید  {ip}کاربر گرامی فرد جدیدی با آدرس آیپی "
                        };
                        userService.sendEmailToUser(refresh.user.Email,text,"هشدار امنیتی");
                    });
                }
                await _next(httpContext);
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityMiddleWare>();
        }
    }
}
