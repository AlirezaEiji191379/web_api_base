using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event_Creator.models;
using Event_Creator.Other.Interfaces;
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
            user.role = Role.User;
            await _appContext.Users.AddAsync(user);
            Random random = new Random();
            int code = random.Next(100000, 999999);
            await _userService.sendEmailToUser(user.Email , code);
            Verification verification = new Verification() {
                User = user,
                VerificationCode = code,
                usage=Usage.SignUp
            };
            await _appContext.verifications.AddAsync(verification);
            await _appContext.SaveChangesAsync();
            return Ok(Information.okSignUp);
        }

















    }
}
