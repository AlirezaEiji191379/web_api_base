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
using Event_Creator.models.Security;
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

        public async Task Invoke(HttpContext httpContext , ApplicationContext dbContext , IUserService userService,IJwtService _jwtService)
        {
            
            
            if (httpContext.User.Identity.IsAuthenticated==false) {
                await _next(httpContext);
            }
            else
            {
                string jti = _jwtService.getJwtIdFromJwt(httpContext);
                /// if the jwt access token was revoked! 
                if (await dbContext.jwtBlackLists.SingleOrDefaultAsync(x => x.jwtToken.Equals(jti)) != null) {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Revoked Token! you must relogin");
                    return;
                }
                RefreshToken refresh = await dbContext.refreshTokens.SingleOrDefaultAsync(x => x.JwtTokenId.Equals(jti));
                string agent = httpContext.Request.Headers.FirstOrDefault(x => x.Key.Contains("User-Agent")).ToString();
                //if (refresh.UserAgent.Equals(agent) == false){
                //    refresh.Revoked = true;
                //    await dbContext.jwtBlackLists.AddAsync(new JwtBlackList() { 
                //        jwtToken=refresh.JwtTokenId
                //    });
                //    httpContext.Response.StatusCode = 403;
                //    await httpContext.Response.WriteAsync("you must relogin");
                //    await dbContext.SaveChangesAsync();
                //    return;
                //}
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
                            Text = $" وارد حساب کاربری شما شده است در صورتی که فرد وارد شده شما نیستید لطفااتمام همه نشست ها را بزنید و پسورد خود را عوض کرده و دوباره وارد شوید  و سیستم${agent} ورود کرده است {ip}کاربر گرامی فرد جدیدی با آدرس آیپی "
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
