using Event_Creator.models;
using Event_Creator.Other;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Event_Creator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VerifyController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IUserService _userService;
        public VerifyController(ApplicationContext applicationContext, IUserService userService)
        {
            _appContext = applicationContext;
            _userService = userService;
        }



        [Route("[action]/{username}/{code}")]
        public async Task<IActionResult> Verify(string username, int code)
        {
            Verification verification = await _appContext.verifications.Include(x => x.User).FirstOrDefaultAsync(a => a.User.Username == username);

            if (verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            if(unixTimeSeconds > verification.expirationTime)
            {
                _appContext.verifications.Remove(verification);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.failedVerification);
            }

            if (verification.usage != Usage.SignUp)
            {
                return BadRequest(Errors.falseVerificationType);
            }
            User user = null;
            if (verification.Requested == 5)
            {

             user = await _appContext.Users.SingleAsync(a => a.Username == username);
             _appContext.Users.Remove(user);
             _appContext.verifications.Remove(await _appContext.verifications.Include(x => x.User).SingleAsync(a => a.User.UserId == user.UserId));
             await _appContext.SaveChangesAsync();
             return BadRequest(Errors.exceedVerification);
            }

            if (verification.VerificationCode != code)
            {
              verification.Requested++;
              _appContext.verifications.Update(verification);
              await _appContext.SaveChangesAsync();
              return BadRequest(Errors.failedVerification);
            }

              user =await _appContext.Users.SingleAsync(a => a.Username == username);
              user.Enable = true;
             _appContext.Users.Update(user);
             _appContext.verifications.Remove(await _appContext.verifications.Include(x => x.User).SingleAsync(a => a.User.UserId == user.UserId));
             await _appContext.SaveChangesAsync();

            return Ok(Information.okVerifySignUp);
        }



        [Route("[action]/{username}")]
        public async Task<IActionResult> ResendCode(string username)
        {
            User user = null;
            Verification verification = await  _appContext.verifications.Include(x => x.User).FirstOrDefaultAsync(a => a.User.Username == username);
            if (verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }

            if (verification.usage != Usage.SignUp)
            {
                return BadRequest(Errors.falseVerificationType);
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

            if (verification.Resended == true)
            {
                user = _appContext.Users.Single(a => a.Username == username);
                _appContext.verifications.Remove(await _appContext.verifications.Include(x => x.User).SingleAsync(a => a.User.UserId == user.UserId));
                _appContext.Users.Remove(user);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.exceedVerification);
            }


            Random random = new Random();
            int code = random.Next(100000, 999999);
            verification.VerificationCode = code;
            verification.Requested = 0;
            verification.Resended = true;
            verification.expirationTime = unixTimeSeconds + 300;/////////
            _appContext.verifications.Update(verification);
            TextPart text = new TextPart("plain")
            {
                Text = $"verification Code is {code} and it is valid for 15 mins!"
            };
            await _appContext.SaveChangesAsync();
            await _userService.sendEmailToUser(user.Email,text);
            return Ok(Information.okResendCode);
        }



    }
}
