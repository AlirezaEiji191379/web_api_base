﻿using Event_Creator.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class UserSignUpRequest
    {
        public User user { get; set; }
        public string captchaToken { get; set; }

    }
}
