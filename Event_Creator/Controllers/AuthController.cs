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
using System.IO;
using Event_Creator.Other.Filters;

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
            User user = await _appContext.Users.Where(x => x.Username.Equals(loginRequest.Username)).SingleOrDefaultAsync();
            if (user == null) return NotFound(Errors.wrongAuth);
            Verification verification = await _appContext.verifications.Include(x => x.User).Where(x => x.User.Username.Equals(loginRequest.Username) && x.usage==Usage.Login).FirstOrDefaultAsync();
            if(verification!=null) return BadRequest("ok");
            if (user.Enable == false)
            {
                return BadRequest(Errors.notEnabledLogin);
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            LockedAccount isLocked = null;
            FailedLogin failedLogin = null;
            isLocked = await _appContext.lockedAccounts.Include(x => x.user).SingleOrDefaultAsync(x => x.user.UserId == user.UserId);
            failedLogin = await _appContext.failedLogins.Include(x => x.user).FirstOrDefaultAsync(x => x.user.Username.Equals(loginRequest.Username));
            if (isLocked != null)
            {
                if (isLocked.unlockedTime > unixTimeSeconds) return StatusCode(429, Errors.failedLoginLock);
                else { 
                  _appContext.lockedAccounts.Remove(isLocked);
                    if (failedLogin != null) _appContext.failedLogins.Remove(failedLogin);
                    await _appContext.SaveChangesAsync();
                }
            }
            if (failedLogin != null && failedLogin.request == 5)
            {
                _appContext.failedLogins.Remove(failedLogin);
                await _appContext.lockedAccounts.AddAsync(new LockedAccount()
                {
                    user = user,
                    unlockedTime = unixTimeSeconds + 300/////////////////////////////////////////
                });
                 await _appContext.SaveChangesAsync();
                return StatusCode(429, Errors.failedLoginLock);
            }

            if (_userService.Check(user.Password, loginRequest.Password) == false)
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
                expirationTime=unixTimeSeconds+300
            };
            if (failedLogin != null) _appContext.failedLogins.Remove(failedLogin);
            await _appContext.verifications.AddAsync(newVerification);
            await _appContext.SaveChangesAsync();
            return Ok("ok");
        }


        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        [ServiceFilter(typeof(CsrfActionFilter))]
        public async Task<IActionResult> logout()
        {
            string jti = _jwtService.getJwtIdFromJwt(HttpContext);
            RefreshToken refreshToken = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.JwtTokenId.Equals(jti));
            if (refreshToken == null) return BadRequest(Errors.NotFoundRefreshToken);
            refreshToken.Revoked = true;
            _appContext.refreshTokens.Update(refreshToken);
            JwtBlackList blackList = new JwtBlackList()
            {
                jwtToken = jti
            };
            await _appContext.jwtBlackLists.AddAsync(blackList);
            await _appContext.SaveChangesAsync();
            return Ok();
        }

        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        [ServiceFilter(typeof(CsrfActionFilter))]
        public async Task<IActionResult> terminateAllSessions()
        {
            string jti = _jwtService.getJwtIdFromJwt(HttpContext);
            RefreshToken refreshToken = await _appContext.refreshTokens.Include(x => x.user).SingleOrDefaultAsync(x => x.JwtTokenId.Equals(jti));
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.UserId == Convert.ToInt64(refreshToken.user.UserId));
            await _appContext.Entry(user).Collection(x => x.RefreshTokens).LoadAsync();
            List<RefreshToken> allUserTokens = user.RefreshTokens.ToList().FindAll(x => x.Priority > refreshToken.Priority);
            for (int i = 0; i < allUserTokens.Count; i++)
            {
                await _appContext.jwtBlackLists.AddAsync(new JwtBlackList()
                {
                    jwtToken = allUserTokens[i].JwtTokenId
                });
                allUserTokens[i].Revoked = true;
            }
            _appContext.refreshTokens.UpdateRange(allUserTokens);
            await _appContext.SaveChangesAsync();
            return Ok();
        }



        [Route("[action]/{priority}")]
        [HttpDelete]
        [Authorize]
        [ServiceFilter(typeof(CsrfActionFilter))]
        public async Task<IActionResult> TerminateOneSession(int priority)
        {
            string jti = _jwtService.getJwtIdFromJwt(HttpContext);
            RefreshToken requester = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.JwtTokenId.Equals(jti));
            if(requester.Priority >= priority)
            {
                return StatusCode(403,"شما به دلایل امنیتی اجازه ندارید مابقی نشست هایی که قبل از شما ورود نموده اند را حذف کنید");
            }
            await _appContext.Entry(requester).Reference(x => x.user).LoadAsync();
            RefreshToken finished = await _appContext.refreshTokens.Where(x => x.user.UserId==requester.user.UserId && x.Priority==priority).SingleOrDefaultAsync();
            if (finished == null) return BadRequest("درخواست اشتباه");
            await _appContext.jwtBlackLists.AddAsync(new JwtBlackList() { 
                jwtToken=finished.JwtTokenId    
            });
            finished.Revoked = true;
            _appContext.refreshTokens.Update(finished);
            await _appContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("[action]")]
        [Authorize]
        [ServiceFilter(typeof(CsrfActionFilter))]
        public async Task<List<DeviceResponse>> getAllDevices()
        {
            long userId = _jwtService.getUserIdFromJwt(HttpContext);
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.UserId == userId);
            await _appContext.Entry(user).Collection(x => x.RefreshTokens).LoadAsync();
            List<RefreshToken> allUserToken = user.RefreshTokens.ToList();
            List<DeviceResponse> allDevices = new List<DeviceResponse>();
            foreach (RefreshToken refresh in allUserToken)
            {
                allDevices.Add(new DeviceResponse() { 
                    priority= refresh.Priority,
                    UserAgent = refresh.UserAgent
                });
            }
            return allDevices;
        }

        [Route("[action]")]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public string test()
        {
            var userAgent = Request.Headers.FirstOrDefault(x => x.Key.Contains("User-Agent"));

            return Request.HttpContext.Connection.RemoteIpAddress.ToString() + "       " + userAgent.ToString();
           // var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images"));
            //return path.ToString();
        }



    }
}
