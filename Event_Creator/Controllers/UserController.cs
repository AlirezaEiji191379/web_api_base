using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event_Creator.models;
using Event_Creator.data;
using Microsoft.AspNetCore.Authorization;
using Event_Creator.Other.Services;
using Event_Creator.Other.Interfaces;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using Event_Creator.Other;

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
            await _appContext.Users.AddAsync(user);
            int code = await _userService.sendEmailToUser(user.Email);
            Verification verification = new Verification() {
                User = user,
                VerificationCode = code
            };
            await _appContext.verifications.AddAsync(verification);
            return Ok(Information.okSignUp);
        }

        [Route("[action]/{username}/{code}")]
        public string Verify(string username , int code)
        {

            return username;
        }


        //[Route("[action]")]
        public string Test()
        {

            return "hi";
        }

















    }
}
