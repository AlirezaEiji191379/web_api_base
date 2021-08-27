using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string captchaToken { get; set; }
    }
}
