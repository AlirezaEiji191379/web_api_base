using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models.Security
{
    public class FailedLogin
    {
        public long FailedLoginId { get; set; }
        public User user { get; set; }
        public int request { get; set; }
    }
}
