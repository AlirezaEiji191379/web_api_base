using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class LockedAccount
    {
        public long LockedAccountId { get; set; }
        public User user { get; set; }
        public Int64 unlockedTime { get; set; }
    }
}
