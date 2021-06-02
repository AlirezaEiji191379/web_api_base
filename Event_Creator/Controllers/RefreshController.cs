using Event_Creator.Other;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
    public class RefreshController : ControllerBase
    {

        private readonly IJwtService _jwtService;

        public RefreshController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> GetRefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            AuthResponse authResponse = await _jwtService.RefreshToken(refreshRequest);
            return StatusCode(authResponse.statusCode,authResponse);
        }



    }
}
