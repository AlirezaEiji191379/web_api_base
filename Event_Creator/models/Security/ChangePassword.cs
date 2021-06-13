using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models.Security
{
    public class ChangePassword
    {
        public long ChangePasswordId { get; set; }
        public string NewPassword { get; set; }
        public User user;
    }
}
