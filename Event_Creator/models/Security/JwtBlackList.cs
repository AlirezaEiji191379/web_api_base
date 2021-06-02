using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models.Security
{
    public class JwtBlackList
    {
        public long JwtBlackListId { get; set;}
        public string jwtToken { get; set; }


    }
}
