﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event_Creator.models;
using Event_Creator.Other.Interfaces;
using Event_Creator.Other;
using MimeKit;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Event_Creator.models.Security;

namespace Event_Creator.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IUserService _userService;
        public UserController (ApplicationContext applicationContext,IUserService userService)
        {
            _appContext = applicationContext;
            _userService = userService;
        }
        


        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {
            List<string> duplicationErrors = await _userService.checkUserDuplicate(user);
            if (duplicationErrors.Count != 0)
            {
                return BadRequest(duplicationErrors);
            }
            user.Enable = false;
            user.role = Role.User;
            user.Password = _userService.Hash(user.Password);
            await _appContext.Users.AddAsync(user);
            Random random = new Random();
            int code = random.Next(100000, 999999);
            TextPart text = new TextPart("plain")
            {
                Text = $"verification Code is {code} and it is valid for 15 mins!"
            };
            await _userService.sendEmailToUser(user.Email,text,"کد تایید ثبت نام");
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            Verification verification = new Verification()
            {
                User = user,
                VerificationCode = code,
                usage = Usage.SignUp,
                expirationTime=unixTimeSeconds+300
            };
            await _appContext.verifications.AddAsync(verification);
            await _appContext.SaveChangesAsync();
            return Ok(Information.okSignUp);
        }


        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> changePassword([FromBody] PasswodChangeRequest changeRequest)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.UserId == Convert.ToInt64(uid.ToString()));
            TextPart text = null;
            if (_userService.Check(user.Password , changeRequest.oldPassword) == false)
            {
                 text = new TextPart("Plain")
                {
                    Text = "کاربر گرامی شخصی قصد تغییر رمز عبور حساب کاربری شما را دارد درصورتی که آن شخص شما نیستید رمز عبور خود را تغییر دهید ."
                };
                await _userService.sendEmailToUser(user.Email,text,"هشدار امنیتی");
                return Forbid(Errors.PasswordChangeFailed);
            }
            Random random = new Random();
            int code = random.Next(100000, 999999);
            text = new TextPart("plain")
            {
                Text = "این کد تایید 15 دقیقه فرصت دارد"+$"  میباشد در صورتی که شما در حال تغییر رمز عبور خود نمیباشید حتما تمامی نشست ها را حذف کرده و رمز عبور خود را عوض کرده و بار دیگر وارد شوید {code} کاربر گرامی کد تایید شما "
            };
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            Verification verification = new Verification()
            {
                expirationTime=unixTimeSeconds+300,
                Requested=0,
                Resended=true,
                usage = Usage.ChangePassword,
                User = user,
                VerificationCode = code
            };
            ChangePassword change = new ChangePassword() { 
                user=user,
                NewPassword=changeRequest.newPassword
            };
            await _appContext.changePassword.AddAsync(change);
            await _appContext.verifications.AddAsync(verification);
            await _appContext.SaveChangesAsync();
            await _userService.sendEmailToUser(user.Email, text, "هشدار امنیتی");
            return Ok();
        }


        [Route("[action]/email")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.Email.Equals(email));
            if (user == null) return BadRequest(Errors.NullEmailResetPassword);
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            Random random = new Random();
            int code = random.Next(100000, 999999);
            Verification verification = new Verification()
            {
                User = user,
                expirationTime = unixTimeSeconds + 300,
                Requested=0,
                Resended=true,
                usage=Usage.ResetPassword,
                VerificationCode =code 
            };
            TextPart text = new TextPart("plain")
            {
                Text = $"کاربر گرامی کد تایید شما برای فراموشی رمز عبور {code} است به هیچ وجه آن را در دسترس کسی قرار ندهید. "
            };
            await _appContext.verifications.AddAsync(verification);
            await _userService.sendEmailToUser(user.Email,text, "هشدار امنیتی");
            await _appContext.SaveChangesAsync();
            return Ok(Information.ResetPassword);
        }
















    }
}
