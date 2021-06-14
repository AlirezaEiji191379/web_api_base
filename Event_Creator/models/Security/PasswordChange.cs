using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models.Security
{
    public class PasswordChange
    {
        public long PasswordChangeId { get; set; }

        public User user { get; set; }
        public string NewPassword { get; set; }
    }
}
