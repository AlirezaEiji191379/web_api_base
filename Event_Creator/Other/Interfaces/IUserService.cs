﻿using Event_Creator.models;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.Interfaces
{
    public interface IUserService
    {
        Task<Dictionary<string, string>> checkUserDuplicate(User user);
        Task sendEmailToUser(string email , TextPart text , string subject);

        string Hash(string password);
        bool Check(string hash, string password);


    }
}
