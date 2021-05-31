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
        Task<int> sendEmailToUser(string email);
    }
}
