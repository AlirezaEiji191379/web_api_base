using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class JwtConfig
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
