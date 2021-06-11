using Event_Creator.models;
using Event_Creator.models.Security;
using Event_Creator.Other;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Event_Creator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly JwtConfig _jwtConfig;
        public AuthController(ApplicationContext applicationContext, IJwtService jwtService , IUserService userService , JwtConfig jwtConfig)
        {
            _appContext = applicationContext;
            _jwtService = jwtService;
            _userService = userService;
            _jwtConfig = jwtConfig;
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.Username.Equals(loginRequest.Username));
            Verification verification = await _appContext.verifications.Include(x => x.User).FirstOrDefaultAsync(x => x.User.Username.Equals(loginRequest.Username));
            //RefreshToken refreshToken = await _appContext.refreshTokens.Include(x => x.user).FirstOrDefaultAsync(x => x.user.Username.Equals(loginRequest.Username));
            if(verification!=null) return BadRequest(Information.okSignUp);
            //if (refreshToken != null && refreshToken.ipAddress.Equals(Request.HttpContext.Connection.RemoteIpAddress.ToString())) return BadRequest(Errors.alreadySignedIn);
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            LockedAccount isLocked = null;
            FailedLogin failedLogin = null;
            if (user != null) isLocked = await _appContext.lockedAccounts.Include(x => x.user).SingleOrDefaultAsync(x => x.user.UserId == user.UserId);
            if (user != null) failedLogin = await _appContext.failedLogins.Include(x => x.user).FirstOrDefaultAsync(x => x.user.Username.Equals(loginRequest.Username));
            if (user != null && user.Enable == false)
            {
                return BadRequest(Errors.notEnabledLogin);
            }

            if (isLocked != null)
            {
                if (isLocked.unlockedTime > unixTimeSeconds) return StatusCode(429, Errors.failedLoginLock);
                else await Task.Run(() => _appContext.lockedAccounts.Remove(isLocked));
            }

            if (failedLogin != null && failedLogin.request == 5)
            {
                _appContext.failedLogins.Remove(failedLogin);
                if (isLocked == null) await _appContext.lockedAccounts.AddAsync(new LockedAccount()
                {
                    user = user,
                    unlockedTime = unixTimeSeconds + 300/////////////////////////////////////////
                });
                else
                {
                    isLocked.unlockedTime = unixTimeSeconds + 300;//////////////////////////////
                    _appContext.lockedAccounts.Update(isLocked);
                }
                 await _appContext.SaveChangesAsync();
                return StatusCode(429, Errors.failedLoginLock);
            }

            if (user == null || _userService.Check(user.Password, loginRequest.Password) == false)
            {
                if (user != null && user.Enable == true)
                {
                    if (failedLogin == null) await _appContext.failedLogins.AddAsync(new FailedLogin()
                    {
                        request = 1,
                        user = user
                    });
                    else
                    {
                        failedLogin.request++;
                        _appContext.failedLogins.Update(failedLogin);
                    }
                    await _appContext.SaveChangesAsync();
                }
                return NotFound(Errors.wrongAuth);
            }

            Random random = new Random();
            int code = random.Next(100000, 999999);
            TextPart text = new TextPart("plain")
            {
                Text = $"verification Code is {code} and it is valid for 15 mins!"
            };
            await _userService.sendEmailToUser(user.Email, text,"کد تایید ورود");
            Verification newVerification = new Verification()
            {
                VerificationCode = code,
                Requested = 0,
                usage = Usage.Login,
                User = user,
                expirationTime=unixTimeSeconds+300////////////
            };

            await _appContext.verifications.AddAsync(newVerification);
            await _appContext.SaveChangesAsync();
            return Ok(Information.okSignIn);
        }


        [Route("[action]/{username}/{code}")]
        public async Task<IActionResult> Verify(string username, int code)
        {
            LockedAccount isLocked = await  _appContext.lockedAccounts.Include(x => x.user).SingleOrDefaultAsync(x => x.user.Username.Equals(username));
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
                   if(isLocked==null)await _appContext.lockedAccounts.AddAsync(locked);
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

            user =await _appContext.Users.SingleAsync(a => a.Username == username);
            RefreshToken refresh = await _appContext.refreshTokens.Include(x => x.user).FirstOrDefaultAsync(x => x.user.UserId == user.UserId);
            if(refresh != null && refresh.Revoked==false)
            {
                string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                TextPart text = new TextPart("plain")
                {
                    Text = $" وارد حساب کاربری شما شده است در صورتی که فرد وارد شده شما نیستید لطفااتمام همه نشست ها را بزنید و پسورد خود را عوض کرده و دوباره وارد شوید  {ip}کاربر گرامی فرد جدیدی با آدرس آیپی "
                };
                Task.Run(() => { _userService.sendEmailToUser(user.Email, text, "هشدار امنیتی"); } );
            }
            _appContext.verifications.Remove(_appContext.verifications.Single(a => a.User.UserId == user.UserId));
            await _appContext.SaveChangesAsync();
            string jwtId = Guid.NewGuid().ToString();
            string jwtAccessToken = await _jwtService.JwtTokenGenerator(user.UserId,jwtId);
            RefreshToken refreshToken = await _jwtService.GenerateRefreshToken(jwtId,user.UserId, Request.HttpContext.Connection.RemoteIpAddress.ToString());
            await _appContext.refreshTokens.AddAsync(refreshToken);
            await _appContext.SaveChangesAsync();
            AuthResponse response = new AuthResponse()
            {
                ErrorList = null,
                success=true,
                RefreshToken=refreshToken.Token,
                JwtAccessToken=jwtAccessToken,
                statusCode=200,
            };
            return Ok(response);
        }



        [Route("[action]/{username}")]
        public async Task<IActionResult> ResendCode(string username)
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
                Verification verification1 = await _appContext.verifications.FirstOrDefaultAsync(x => x.User.Username.Equals(username)); 
                _appContext.verifications.Remove(verification1);
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
            await _userService.sendEmailToUser(user.Email, text,"کد تایید ورود");
            return Ok(Information.okResendCode);
        }


        
        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> logout()
        {
            var authorizationHeader =Request.Headers.Single(x => x.Key=="Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var jti = tokenS.Claims.First(claim => claim.Type == "jti").Value;
            RefreshToken refreshToken = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.JwtTokenId.Equals(jti));
            if (refreshToken == null) return BadRequest(Errors.NotFoundRefreshToken);
            refreshToken.Revoked = true;
            _appContext.refreshTokens.Update(refreshToken);
            JwtBlackList blackList = new JwtBlackList()
            {
                jwtToken = stream
            };
            await _appContext.jwtBlackLists.AddAsync(blackList);
            await _appContext.SaveChangesAsync();
            return Ok();
        }

        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> TerminateAllSessions()
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.UserId == long.Parse(uid));
            await _appContext.Entry(user).Collection(x => x.RefreshTokens).LoadAsync();
            List<RefreshToken> allUserTokens = user.RefreshTokens.ToList();
            for(int i = 0; i < allUserTokens.Count; i++)
            {
                allUserTokens[i].Revoked = true;
                _appContext.refreshTokens.Update(allUserTokens[i]);
            }
            await _appContext.SaveChangesAsync();
            return Ok();
        }



        [Route("[action]")]
        [Authorize(Roles ="User")]
        public string test()
        {
            var userAgent = Request.Headers.FirstOrDefault(x => x.Key.Contains("User-Agent"));
            return Request.HttpContext.Connection.RemoteIpAddress.ToString();
        }




    }
}
