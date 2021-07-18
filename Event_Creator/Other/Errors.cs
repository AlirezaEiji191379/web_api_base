using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public static class Errors
    {

        public static string InvalidFileFormat
        {
            get { return "فرمت ارسالی عکس ها باید jpg و یا png باشد"; }
        }

        public static string InvalidBookJsonFormat
        {
            get { return "فرمت ارسالی برای کتاب ها نا درست هستند!"; }
        }
        public static string InvalidCategoryName
        {
            get { return "چنین دسته بندی وجود ندارد"; }
        }
        public static string InvalidParentCategory
        {
            get { return "چنین دسته بندی مادری وجود ندارد"; }
        }
        public static string exceedLogin {
            get { return "لطفا مجددا اقدام به ورود نمایید"; }
        }


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

        public static string NullVerification
        {
            get { return "این نام کاربری در سیستم تایید وجود ندارد"; }
        }

        public static string failedVerification
        {
            get { return "کد تایید وارد شده نادرست است."; }
        }

        public static string exceedVerification
        {
            get { return "کاربر گرامی لطفا مجددا اقدام به ثبت نام نمایید"; }
        }

        public static string wrongAuth
        {
            get { return "نام کاربری یا رمز عبور اشتباه است."; }
        }

        public static string falseVerificationType
        {
            get { return "خطا در تایید کد"; }
        }

        public static string notEnabledLogin
        {
            get { return "کاربر گرامی لطفا اکانت خود را تایید نمایید"; }
        }

        public static string failedLoginLock
        {
            get { return "کاربر گرامی لطفا بعدا اقدام به ورود نمایید"; }
        }

        public static string NotFoundRefreshToken
        {
            get { return "رفرش توکن نا معتبر است"; }
        }

        public static string InvalidJwtToken
        {
            get { return "اکسس توکن نامعتبر است"; }
        }

        public static string NotExpiredToken
        {
            get { return "توکن شما هنوز منقضی نشده است"; }
        }

        public static string RevokedToken
        {
            get { return "این رفرش توکن باطل شده است"; }
        }

        public static string refreshTokenExpired
        {
            get { return "کاربر گرامی مجددا اقدام به ورود نمایید"; }
        }

        public static string expiredVerification
        {
            get { return "کاربر گرامی لطفا بعدا اقدام به ورود نمایید"; }
        }

        public static string alreadySignedIn
        {
            get { return "شما قبلا وارد شده اید"; }
        }
        
        public static string PasswordChangeFailed
        {
            get { return "رمز عبور اشتباه است"; }
        }

        public static string NullEmailResetPassword
        {
            get { return "این ایمیل در سیستم وجود ندارد"; }
        }

    }
}
