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
using Microsoft.AspNetCore.Authorization;
using Event_Creator.models.Security;
using System.Security.Cryptography;

namespace Event_Creator.Controllers
{
    //checked!
    [Route("[controller]")]
    [ApiController]
    public class VerifyController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        public VerifyController(ApplicationContext applicationContext, IUserService userService , IJwtService jwtService)
        {
            _appContext = applicationContext;
            _userService = userService;
            _jwtService = jwtService;
        }


        [Route("[action]/{username}/{code}")]
        public async Task<IActionResult> VerifySignUp(string username, int code)
        {
            Verification verification = await _appContext.verifications.Include(x => x.User).FirstOrDefaultAsync(a => a.User.Username == username);

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
            if (unixTimeSeconds > verification.expirationTime)
            {
                _appContext.verifications.Remove(verification);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.failedVerification);
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


        [Route("[action]/{username}/{code}")]
        public async Task<IActionResult> VerifyLogin(string username, int code)
        {
            LockedAccount isLocked = await _appContext.lockedAccounts.Include(x => x.user).SingleOrDefaultAsync(x => x.user.Username.Equals(username));
            Verification verification = await _appContext.verifications.Include(x => x.User).FirstOrDefaultAsync(x => x.User.Username.Equals(username));

            if (verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }

            if (verification.usage != Usage.Login)
            {
                return BadRequest(Errors.falseVerificationType);
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            if (unixTimeSeconds > verification.expirationTime)
            {
                _appContext.verifications.Remove(verification);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.expiredVerification);
            }
            User user = null;
            if (verification.Requested == 5)
            {
                user = await _appContext.Users.SingleAsync(a => a.Username == username);
                LockedAccount locked = new LockedAccount()
                {
                    user = user,
                    unlockedTime = unixTimeSeconds + 300
                };
                if (isLocked == null) await _appContext.lockedAccounts.AddAsync(locked);
                else
                {
                    isLocked.unlockedTime = unixTimeSeconds + 300;
                    _appContext.lockedAccounts.Update(isLocked); ////////////////////////////////////////////
                }
                _appContext.verifications.Remove(await _appContext.verifications.FirstAsync(a => a.User.UserId == user.UserId));
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

            user = await _appContext.Users.SingleAsync(a => a.Username == username);
            RefreshToken refresh = await _appContext.refreshTokens.Include(x => x.user).FirstOrDefaultAsync(x => x.user.UserId == user.UserId);
            if (refresh != null && refresh.Revoked == false)
            {
                string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                TextPart text = new TextPart("plain")
                {
                    Text = $" وارد حساب کاربری شما شده است در صورتی که فرد وارد شده شما نیستید لطفااتمام همه نشست ها را بزنید و پسورد خود را عوض کرده و دوباره وارد شوید  {ip}کاربر گرامی فرد جدیدی با آدرس آیپی "
                };
                Task.Run(() => { _userService.sendEmailToUser(user.Email, text, "هشدار امنیتی"); });
            }
            _appContext.verifications.Remove(_appContext.verifications.Single(a => a.User.UserId == user.UserId));
            await _appContext.SaveChangesAsync();
            string jwtId = Guid.NewGuid().ToString();
            string jwtAccessToken = await _jwtService.JwtTokenGenerator(user.UserId, jwtId);
            RefreshToken refreshToken = await _jwtService.GenerateRefreshToken(jwtId, user.UserId, HttpContext);
            await _appContext.refreshTokens.AddAsync(refreshToken);
            await _appContext.SaveChangesAsync();
            AuthResponse response = new AuthResponse()
            {
                ErrorList = null,
                success = true,
                RefreshToken = refreshToken.Token,
                JwtAccessToken = jwtAccessToken,
                statusCode = 200,
            };
            return Ok(response);
        }


        [Authorize]
        [Route("[action]/{username}/{code}")]
        public async Task<IActionResult> VerifyChangePassword(string username, int code)
        {
            Verification verification = await _appContext.verifications.Include(x => x.User).SingleOrDefaultAsync(x => x.User.Username.Equals(username));
            if (verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }

            if (verification.usage != Usage.ChangePassword)
            {
                return BadRequest(Errors.falseVerificationType);
            }
            ChangePassword change = null;
            if (verification.Requested == 2)
            {
                change = await _appContext.changePassword.Include(x => x.user).SingleOrDefaultAsync(x => x.user.Username.Equals(username));
                _appContext.verifications.Remove(verification);
                _appContext.changePassword.Remove(change);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.exceedVerification);
            }

            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            if (unixTimeSeconds > verification.expirationTime)
            {
                change = await _appContext.changePassword.Include(x => x.user).SingleOrDefaultAsync(x => x.user.Username.Equals(username)); 
                _appContext.verifications.Remove(verification);
                _appContext.changePassword.Remove(change);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.expiredVerification);
            }

            if (verification.VerificationCode != code)
            {
                verification.Requested++;
                _appContext.verifications.Update(verification);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.failedVerification);
            }
            await _appContext.Entry(verification.User).Collection(x => x.RefreshTokens).LoadAsync();
            List<RefreshToken> allUserTokens = verification.User.RefreshTokens.ToList(); ;
            for (int i = 0; i < allUserTokens.Count; i++)
            {
                await _appContext.jwtBlackLists.AddAsync(new JwtBlackList()
                {
                    jwtToken = allUserTokens[i].JwtTokenId
                });
                allUserTokens[i].Revoked = true;
                //_appContext.refreshTokens.Update(allUserTokens[i]);
            }
            _appContext.refreshTokens.UpdateRange(allUserTokens);
            change = await _appContext.changePassword.Include(x => x.user).SingleOrDefaultAsync(x => x.user.Username.Equals(username));
            change.user.Password = _userService.Hash(change.NewPassword);
            _appContext.Users.Update(change.user);
            await _appContext.SaveChangesAsync();
            return Ok(Information.SuccessChangePassword);
        }


        [Route("[action]/{email}/{code}")]
        public async Task<IActionResult> VerifyForgetPassword(string email ,int code)
        {
            Verification verification = await _appContext.verifications.Include(x => x.User).SingleOrDefaultAsync(x => x.User.Email.Equals(email));
            if(verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }
            if (verification.usage != Usage.ResetPassword)
            {
                return BadRequest(Errors.falseVerificationType);
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            if (unixTimeSeconds > verification.expirationTime)
            {
                _appContext.verifications.Remove(verification);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.expiredVerification);
            }
            if (verification.Requested == 1)
            {
                _appContext.verifications.Remove(verification);
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
            byte[] rgb = new byte[9];
            RNGCryptoServiceProvider rngCrypt = new RNGCryptoServiceProvider();
            rngCrypt.GetBytes(rgb);
            string newPassword = Convert.ToBase64String(rgb);
            verification.User.Password = _userService.Hash(newPassword);
            _appContext.Users.Update(verification.User);
            await _appContext.SaveChangesAsync();
            TextPart text = new TextPart("plain") {
                Text = $"رمز عبور جدید شما {newPassword} میباشد. "
            };
            await _userService.sendEmailToUser(verification.User.Email,text,"رمز عبور جدید");
            return Ok(Information.ResetPassword);
        }



        [Route("[action]/{username}")]
        public async Task<IActionResult> ResendCodeSignUp(string username)
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
                _appContext.verifications.Remove(verification);
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
            await _userService.sendEmailToUser(user.Email,text, "کد تایید ثبت نام");
            return Ok(Information.okResendCode);
        }


        [Route("[action]/{username}")]
        public async Task<IActionResult> ResendCodeLogin(string username)
        {
            User user = null;
            Verification verification = await _appContext.verifications.Include(x => x.User).FirstOrDefaultAsync(x => x.User.Username.Equals(username));
            if (verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }

            if (verification.usage != Usage.Login)
            {
                return BadRequest(Errors.falseVerificationType);
            }

            if (verification.Resended == true)
            {
                _appContext.verifications.Remove(verification);
                await _appContext.SaveChangesAsync();
                return BadRequest(Errors.exceedLogin);
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

            Random random = new Random();
            int code = random.Next(100000, 999999);
            verification.VerificationCode = code;
            verification.Requested = 0;
            verification.Resended = true;
            verification.expirationTime = unixTimeSeconds + 300; ////////
            _appContext.verifications.Update(verification);
            TextPart text = new TextPart("plain")
            {
                Text = $"verification Code is {code} and it is valid for 5 mins!"
            };
            await _appContext.SaveChangesAsync();
            await _userService.sendEmailToUser(user.Email, text, "کد تایید ورود");
            return Ok(Information.okResendCode);
        }


    }
}
