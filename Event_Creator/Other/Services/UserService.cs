using Event_Creator.models;
using Event_Creator.Other.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kavenegar.Models;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
namespace Event_Creator.Other.Services
{
    public class UserService : IUserService
    {

        private readonly ApplicationContext _applicationContext;
        public UserService(ApplicationContext app)
        {
            _applicationContext = app;
        }


        public Task<List<string>> checkUserDuplicate(User user)
        {
            return Task.Run(() =>
            {
                 List<string> allDuplications = new List<string>();

                 if (_applicationContext.Users.SingleOrDefault(a => a.Username == user.Username) != null) allDuplications.Add(Errors.UsernameDuplication);

                 if (_applicationContext.Users.SingleOrDefault(a => a.Email == user.Email) != null) allDuplications.Add(Errors.EmailDuplication);

                 if (_applicationContext.Users.SingleOrDefault(a => a.PhoneNumber == user.PhoneNumber) != null) allDuplications.Add(Errors.PhoneDuplication);

                 return allDuplications;
            }
           );
        }

        public Task sendSmsToUser(string phoneNumber)
        {
            var sender = "1000596446";
            var receptor = "09194165232";
            var message = ".وب سرویس پیام کوتاه کاوه نگار";
            var api = new Kavenegar.KavenegarApi("663842567070592B752F44723656626646756657496377744668762F43446134725074777065506B4D6B633D");
            return Task.Run(() => api.Send(sender, receptor, message));
        }



        public async Task sendEmailToUser(string email , int code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Event_Creator Team Support", "alirezaeiji191379@gmail.com"));
            message.To.Add(new MailboxAddress("Event_Creator Client", email));
            message.Subject = "کد تایید";
            message.Body = new TextPart("plain")
            {
                Text = $"verification Code is {code}"
            };
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("alirezaeiji191379@gmail.com", "aqvootqbfziukjce");
                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }

    }
}
