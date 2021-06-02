using Event_Creator.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.Interfaces
{
    public interface IUserService
    {
        Task<List<string>> checkUserDuplicate(User user);
        Task sendEmailToUser(string email , int code);

        string Hash(string password);
        bool Check(string hash, string password);
    }
}
