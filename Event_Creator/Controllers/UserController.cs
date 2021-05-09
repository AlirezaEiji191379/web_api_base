using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event_Creator.models;
using System.Net.Mail;
using Event_Creator.data;
namespace Event_Creator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        /// <summary>
        /// constructor with dependacy injection!
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public UserController(ApplicationContext applicationContext)
        {
            this._appContext = applicationContext;
        }



        /// <summary>
        /// getting user data send email and saving in database
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Register([FromBody]User user)
        {
            string badreq = "";
            user.Enable = false;
            Random random = new Random();
            user.ConfrimCode = random.Next(100000,999999);
            Task<bool> saveToDbAndSendEmail = Task.Run(() =>
            {
                try
                {
                    _appContext.Users.Add(user);
                    _appContext.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    badreq = "کاربر گرامی لطفا ایرادات را برطرف نمایید";
                    return false;
                }
            });
            await saveToDbAndSendEmail;
            if (saveToDbAndSendEmail.Result == false) return BadRequest(badreq);
            else {
                Task.Run(()=>sendEmail(user.Email,user.ConfrimCode));
                return Ok(); 
            }
        }


        /// <summary>
        /// checking the duplication of unique cols in database!
        /// </summary>
        /// <param name="check"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("validate")]
        public async Task<ActionResult> ValidUniqueParamsInRequest(string check,string value)
        {
            Task<string> checkDuplicate = Task.Run(()=> {
                

                if (check.Equals("Email"))
                {
                    if (_appContext.Users.Single(x => x.Email==value)!=null) { return "این پست الکترونیکی قبلا ثبت شده است"; }

                }else if (check.Equals("Username"))
                {
                    if (_appContext.Users.Single(x => x.Username == value) != null) { return "این نام کاربری قبلا ثبت شده است"; }
                }
                else if (check.Equals("PhoneNumber"))
                {
                    if (_appContext.Users.Single(x => x.PhoneNumber == value) != null) { return "این شماره قبلا ثبت شده است"; }
                }
                else
                {
                    return "درخواست نامعتبر است";
                }
                return "";
            });

            await checkDuplicate;
            if (checkDuplicate.Result.Equals(""))
            {
                return Ok();
            }
            else if(checkDuplicate.Result.Equals("درخواست نامعتبر است"))
            {
                return BadRequest(checkDuplicate.Result);
            }
            else
            {
                return Forbid(checkDuplicate.Result);
            }

        }






        /// <summary>
        /// / sending email with confirmation code for user! isOk is for the result of data saving in database!
        /// </summary>
        /// <param name="UserEmail"></param>
        /// <param name="code"></param>
        [NonAction]
        private void sendEmail(string UserEmail,int code)
        {
            //if (isOk == false) return isOk;
            MailMessage mail = new MailMessage();
            mail.To.Add(UserEmail);
            mail.From = new MailAddress("alirezaeiji191379@gmail.com");
            mail.Subject = "Event Creator Confirmation Code";
            string Body = "your confirmation code is "+code.ToString();
            mail.Body = Body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com"; //Or Your SMTP Server Address
            smtp.Credentials = new System.Net.NetworkCredential
                 ("alirezaeiji191379@gmail.com", "aqvootqbfziukjce");
            //Or your Smtp Email ID and Password
            smtp.EnableSsl = true;
            try
            {
                smtp.Send(mail);
            }
            catch (SmtpFailedRecipientException ex)
            {

            }
        }


    }
}
