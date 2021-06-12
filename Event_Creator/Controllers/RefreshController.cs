using Event_Creator.models;
using Event_Creator.Other;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Event_Creator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RefreshController : ControllerBase
    {

        private readonly IJwtService _jwtService;
        private readonly ApplicationContext _appContext;
        public RefreshController(IJwtService jwtService , ApplicationContext applicationContext)
        {
            _jwtService = jwtService;
            _appContext = applicationContext;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> GetRefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            AuthResponse authResponse = await _jwtService.RefreshToken(refreshRequest,HttpContext);
            return Ok(authResponse);
        }




    }
}
