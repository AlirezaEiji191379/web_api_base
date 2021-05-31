using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public static class Errors
    {
        public static string EmailDuplication
        {
            get { return "این پست الکترونیکی قبلا ثبت شده است"; }
        }

        public static string PhoneDuplication
        {
            get { return "این شماره تلفن قبلا ثبت شده است"; }
        }

        public static string UsernameDuplication
        {
            get { return "این نام کاربری قبلا ثبت شده است"; }
        }

    }
}
