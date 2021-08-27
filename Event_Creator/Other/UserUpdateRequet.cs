using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class UserUpdateRequet
    {
        public UserUpdateField updateField { get; set; }
        public string value { get; set; }
    }

    public enum UserUpdateField
    {
        firstname,
        lastname,
        address
    }

}
