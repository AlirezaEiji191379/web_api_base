using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class RefreshRequest
    {
        public string jwtAccessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
