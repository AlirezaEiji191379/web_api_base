using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class SignUpDuplicationError
    {
        public Dictionary<string,string> errors { get; set; }
        public int statusCode { get; set; }
    }
}
