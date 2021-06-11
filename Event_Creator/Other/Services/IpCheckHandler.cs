using Event_Creator.models;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.Services
{
    public class IpCheckHandler : AuthorizationHandler<IpRequirement>
    {
        private readonly ApplicationContext _appContext;
        private readonly IUserService _userService;
        private IHttpContextAccessor HttpContextAccessor { get; }
        private HttpContext HttpContext => HttpContextAccessor.HttpContext;
        public IpCheckHandler(ApplicationContext contexts ,IHttpContextAccessor httpContextAccessor , IUserService userService)
        {
            _appContext = contexts;
            HttpContextAccessor = httpContextAccessor;
            _userService = userService;
        }


        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IpRequirement requirement)
        {
            var jti = context.User.Claims.FirstOrDefault(x => x.Type=="jti");
            if (jti == null)  context.Fail();
            RefreshToken refreshToken = _appContext.refreshTokens.SingleOrDefault(x => x.JwtTokenId==jti.ToString());
            if (refreshToken == null) context.Fail();
            Task.Run(() => {
            if (refreshToken != null)
            {
                _appContext.Entry(refreshToken).Reference(x => x.user).Load();
                string ip = HttpContext.Connection.RemoteIpAddress.ToString();
                    TextPart text = new TextPart("plain")
                    {
                        Text = $" وارد حساب کاربری شما شده است در صورتی که فرد وارد شده شما نیستید لطفااتمام همه نشست ها را بزنید و پسورد خود را عوض کرده و دوباره وارد شوید  {ip}کاربر گرامی فرد جدیدی با آدرس آیپی "
                    };
                    _userService.sendEmailToUser(refreshToken.user.Email,text,"هشدار امنیتی");
               }

            });
            return Task.CompletedTask;
        }
    }
}
