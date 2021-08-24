using Event_Creator.models;
using Event_Creator.Other;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
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
        private IAntiforgery _antiForgery;
        private readonly IJwtService _jwtService;
        private readonly ApplicationContext _appContext;
        public RefreshController(IJwtService jwtService , ApplicationContext applicationContext, IAntiforgery antiForgery)
        {
            _jwtService = jwtService;
            _appContext = applicationContext;
            _antiForgery = antiForgery;
        }

        [Route("Mobile")]
        [HttpPut]
        public async Task<IActionResult> GetRefreshTokenMobile([FromBody] RefreshRequest refreshRequest)
        {
            AuthResponseMobile authResponse = await _jwtService.RefreshTokenMobile(refreshRequest,HttpContext);
            _antiForgery.GetAndStoreTokens(HttpContext);
            return StatusCode(authResponse.statusCode, new { auth = authResponse });
        }

        [Route("Web")]
        [HttpPut]
        public async Task<IActionResult> GetRefreshTokenWeb()
        {
            AuthResponseWeb authResponse = await _jwtService.RefreshTokenWeb(this.HttpContext);
            _antiForgery.GetAndStoreTokens(HttpContext);
            if (authResponse.statusCode == 200) return Ok();
            else return StatusCode(authResponse.statusCode, new { auth = authResponse });
        }




    }
}
