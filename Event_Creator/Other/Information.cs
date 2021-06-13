using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public static class Information
    {
        public static string okSignUp { get { return "کاربر گرامی کد تاییدی به پست الکترونیکی شما ارسال شده است."; } }
        public static string okSignIn { get { return "کاربر گرامی کد تاییدی به پست الکترونیکی شما ارسال شده است."; } }
        public static string okVerifySignUp { get { return "اکانت شما تایید شد می توانید وارد اکانت شوید"; } }
        public static string okResendCode { get { return "کد تایید جدید برای پست الکترونیکی شما ارسال شده است"; } }
        public static string okPasswordChange { get { return "کاربر گرامی پست الکترونیکی برای شما ارسال شده است"; } }
        public static string SuccessChangePassword { get { return "کاربر گرامی پسورد شما با موفقیت عوض شد لطفا دوباره وارد شوید"; } }
    }
}
