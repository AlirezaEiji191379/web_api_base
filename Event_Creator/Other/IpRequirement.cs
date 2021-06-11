using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class IpRequirement : IAuthorizationRequirement
    {

        public string ipAdderss { get; set; }
        public IpRequirement()
        {
        }



    }
}
