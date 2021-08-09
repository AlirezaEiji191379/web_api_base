using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.Filters
{
    public class CsrfActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
                // here is the code for csrf prevention!
                if (context.HttpContext.Request.Headers.ContainsKey("Authorization") == false)
                {
                    String csrf_cookie = context.HttpContext.Request.Cookies["CSRF-TOKEN"];
                    String csrf_header = context.HttpContext.Request.Headers["X-CSRF-Header"];
                    if (csrf_cookie == null || csrf_header == null)
                    {
                        context.Result = new BadRequestObjectResult("bad request");
                        return;
                    }
                    if (csrf_header.Equals(csrf_cookie) == false)
                    {
                        context.Result = new BadRequestObjectResult("bad request");
                        return;
                    }
                }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //throw new NotImplementedException();
        }
    }
}
