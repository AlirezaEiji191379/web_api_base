﻿using Event_Creator.models;
using Event_Creator.Other;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;

        public AuthController(ApplicationContext applicationContext, IJwtService jwtService , IUserService userService)
        {
            _appContext = applicationContext;
            _jwtService = jwtService;
            _userService = userService;
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            User user =await Task.Run(() =>
            {
                return _appContext.Users.SingleOrDefault(x => x.Username.Equals(loginRequest.Username));
            }
            );
            if(user == null || user.Password.Equals(loginRequest.Password)==false)
            {
                return NotFound(Errors.wrongAuth);
            }

            if (user.Enable == false)
            {
                return Forbid(Errors.notEnabledLogin);
            }
            Random random = new Random();
            int code = random.Next(100000, 999999);
            await _userService.sendEmailToUser(user.Email,code);
            Verification verification = new Verification()
            {
                VerificationCode = code,
                Requested=0,
                usage=Usage.Login,
                User=user
            };

             await _appContext.verifications.AddAsync(verification);
             await _appContext.SaveChangesAsync();
            return Ok(Information.okSignIn);
        }


        [Route("[action]/{username}/{code}")]
        public async Task<IActionResult> Verify(string username, int code)
        {
            Verification verification = await Task.Run(() => {
                return _appContext.verifications.FirstOrDefault(a => a.User.Username == username);
            });

            if (verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }

            if (verification.usage != Usage.Login)
            {
                return BadRequest(Errors.falseVerificationType);
            }
            User user = null;
            if (verification.Requested == 5)
            {
                await Task.Run(() => {
                    user = _appContext.Users.Single(a => a.Username == username);
                    //_appContext.Users.Remove(user); add to locked account
                    _appContext.verifications.Remove(_appContext.verifications.Single(a => a.User.UserId == user.UserId));
                    _appContext.SaveChanges();
                });
                return BadRequest(Errors.exceedVerification);
            }

            if (verification.VerificationCode != code)
            {
                await Task.Run(() => {
                    verification.Requested++;
                    _appContext.verifications.Update(verification);
                    _appContext.SaveChanges();
                });
                return BadRequest(Errors.failedVerification);
            }

            await Task.Run(() => {
                user = _appContext.Users.Single(a => a.Username == username);
                _appContext.verifications.Remove(_appContext.verifications.Single(a => a.User.UserId == user.UserId));
                _appContext.SaveChanges();
            });

            string jwtId = Guid.NewGuid().ToString();
            string jwtAccessToken = _jwtService.JwtTokenGenerator(user.UserId,jwtId);
            RefreshToken refreshToken = await _jwtService.GenerateRefreshToken(jwtId,user.UserId);
            await _appContext.refreshTokens.AddAsync(refreshToken);
            await _appContext.SaveChangesAsync();
            AuthResponse response = new AuthResponse()
            {
                ErrorList = null,
                success=true,
                RefreshToken=refreshToken.Token,
                JwtAccessToken=jwtAccessToken
            };
            return Ok(response);
        }



        [Route("[action]/{username}")]
        public async Task<IActionResult> ResendCode(string username)
        {
            User user = null;
            Verification verification = await Task.Run(() => {
                user = _appContext.Users.SingleOrDefault(a => a.Username == username);
                return _appContext.verifications.FirstOrDefault(a => a.User.Username == username);
            });
            if (verification == null)
            {
                return BadRequest(Errors.NullVerification);
            }

            if (verification.Resended == true)
            {
                await Task.Run(() => {
                    User user = _appContext.Users.Single(a => a.Username == username);
                    _appContext.verifications.Remove(_appContext.verifications.Single(a => a.User.UserId == user.UserId));
                    _appContext.SaveChanges();
                });
                return BadRequest(Errors.exceedVerification);
            }

            if (verification.usage != Usage.Login)
            {
                return BadRequest(Errors.falseVerificationType);
            }

            Random random = new Random();
            int code = random.Next(100000, 999999);
            await Task.Run(() => {
                verification.VerificationCode = code;
                verification.Requested = 0;
                verification.Resended = true;
                _appContext.verifications.Update(verification);
                _appContext.SaveChanges();
            });
            await _userService.sendEmailToUser(user.Email, code);
            return Ok(Information.okResendCode);
        }




        [Route("[action]")]
        public string Test()
        {
            return _jwtService.JwtTokenGenerator(22,Guid.NewGuid().ToString());
        } 





    }
}