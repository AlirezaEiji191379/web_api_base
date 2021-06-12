using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class RefreshToken
    {
        public long RefreshTokenId { get; set; }
        public string JwtTokenId { get; set; }
        public User user { get; set; }
        public Int64 expirationTime { get; set; }
        public string Token { get; set; }
        public bool Revoked { get; set; }

        public string ipAddress { get; set; }

        public string UserAgent {get;set;}

    }
}
