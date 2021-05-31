using Event_Creator.models;
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
        private readonly JwtConfig _jwtConfig; 
        private readonly IUserService _userService;

        public AuthController(ApplicationContext applicationContext, JwtConfig jwt , IUserService userService)
        {
            _appContext = applicationContext;
            _jwtConfig = jwt;
            _userService = userService;
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            User user =await Task.Run(() =>
            {
                return _appContext.Users.SingleOrDefault(x => x.Username == loginRequest.Username);
            }
            );
            if(user == null || user.Password.Equals(loginRequest.Password)==false)
            {
                return NotFound(Errors.wrongAuth);
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

            if (verification.Requested == 5)
            {
                await Task.Run(() => {
                    User user = _appContext.Users.Single(a => a.Username == username);
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
                User user = _appContext.Users.Single(a => a.Username == username);
                _appContext.verifications.Remove(_appContext.verifications.Single(a => a.User.UserId == user.UserId));
                _appContext.SaveChanges();
            });

            // get new Jwt token!
            
            return Ok(Information.okVerifySignUp);
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







    }
}
