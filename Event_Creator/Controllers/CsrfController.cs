using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Controllers
{
    [Route("Web/Csrf")]
    [ApiController]
    public class CsrfController : ControllerBase
    {

        private IAntiforgery _antiForgery;
        public CsrfController(IAntiforgery antiForgery)
        {
            _antiForgery = antiForgery;
        }

        [Route("")]
        public IActionResult GenerateAntiForgeryTokens()
        {
            var tokens = _antiForgery.GetAndStoreTokens(HttpContext);
            return NoContent();
        }

    }
}
