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
                jwtToken = jti
            };
            await _appContext.jwtBlackLists.AddAsync(blackList);
            await _appContext.SaveChangesAsync();
            return Ok();
        }

        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> terminateAllSessions()
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var jti = tokenS.Claims.First(claim => claim.Type == "jti").Value;
            RefreshToken refreshToken = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.JwtTokenId == jti.ToString());
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
        public async Task<IActionResult> TermianteOneSession(int priority)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var jti = tokenS.Claims.First(claim => claim.Type == "jti").Value;
            RefreshToken requester = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.JwtTokenId.Equals(jti.ToString()));
            if(requester.Priority > priority)
            {
                return Forbid("شما به دلایل امنیتی اجازه ندارید مابقی نشست هایی که قبل از شما ورود نموده اند را حذف کنید");
            }
            await _appContext.Entry(requester).Reference(x => x.user).LoadAsync();
            RefreshToken finished = await _appContext.refreshTokens.Where(x => x.user.UserId==requester.user.UserId && x.Priority==priority).SingleOrDefaultAsync();
            await _appContext.jwtBlackLists.AddAsync(new JwtBlackList() { 
                jwtToken=finished.JwtTokenId    
            });
            finished.Revoked = true;
            _appContext.refreshTokens.Update(finished);
            await _appContext.SaveChangesAsync();
            return Ok();
        }


        [Route("[action]")]
        [Authorize]
        public async Task<List<DeviceResponse>> getAllInDevices()
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.UserId == Convert.ToInt64(uid));
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
        [Authorize(Roles ="User")]
        public string test()
        {
            var userAgent = Request.Headers.FirstOrDefault(x => x.Key.Contains("User-Agent"));
            return Request.HttpContext.Connection.RemoteIpAddress.ToString();
        }




    }
}
