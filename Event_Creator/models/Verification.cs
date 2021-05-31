using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Verification
    {
        public long VerificationId { get; set; }
        public User User { get; set; }
        public int VerificationCode { get; set; }
        public int Requested { get; set; }

    }
}
