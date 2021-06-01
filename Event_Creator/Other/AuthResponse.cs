using Event_Creator.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class AuthResponse
    {
        public string JwtAccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool success { get; set; }
        public List<string> ErrorList { get; set; }
    }
}
