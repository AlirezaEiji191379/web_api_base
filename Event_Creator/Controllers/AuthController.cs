using Event_Creator.models;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserService _userService;

        public AuthController(ApplicationContext applicationContext, IUserService userService)
        {
            _appContext = applicationContext;
            _userService = userService;
        }





    }
}
