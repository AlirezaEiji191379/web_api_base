using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.Interfaces
{
    public interface ICaptchaService
    {
      Task<bool> IsCaptchaValid(string token);
    }
}
