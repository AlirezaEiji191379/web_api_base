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
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
namespace Event_Creator.Other.Services
{
    public class UserService : IUserService
    {

        private readonly ApplicationContext _applicationContext;
        private readonly HashingOptions _options;
        private const int SaltSize = 16; // 128 bit 
        private const int KeySize = 32; // 256 bit
        public UserService(ApplicationContext app , IOptions<HashingOptions> options)
        {
            _applicationContext = app;
            _options = options.Value;
        }


        public async Task<Dictionary<string, string>> checkUserDuplicate(User user)
        {

                 Dictionary<string,string> allDuplications = new Dictionary<string, string>();

                 if (await _applicationContext.Users.SingleOrDefaultAsync(a => a.Username == user.Username) != null) allDuplications.Add("Username",Errors.UsernameDuplication);

                 if (await _applicationContext.Users.SingleOrDefaultAsync(a => a.Email == user.Email) != null) allDuplications.Add("Email",Errors.EmailDuplication);

                 if (await _applicationContext.Users.SingleOrDefaultAsync(a => a.PhoneNumber == user.PhoneNumber) != null) allDuplications.Add("Phonenumber",Errors.PhoneDuplication);

                 return allDuplications;

        }

        public Task sendSmsToUser(string phoneNumber)
        {
            var sender = "1000596446";
            var receptor = "09194165232";
            var message = ".وب سرویس پیام کوتاه کاوه نگار";
            var api = new Kavenegar.KavenegarApi("663842567070592B752F44723656626646756657496377744668762F43446134725074777065506B4D6B633D");
            return Task.Run(() => api.Send(sender, receptor, message));
        }



        public async Task sendEmailToUser(string email ,TextPart text , string subject)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Event_Creator Team Support", "alirezaeiji191379@gmail.com"));
            message.To.Add(new MailboxAddress("Event_Creator Client", email));
            message.Subject = subject;
            message.Body = text;
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("alirezaeiji191379@gmail.com", "aqvootqbfziukjce");
                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }



        public string Hash(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              SaltSize,
              _options.Iterations,
              HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{_options.Iterations}.{salt}.{key}";
            }
        }


        public bool Check(string hash, string password)
        {
            var parts = hash.Split('.', 3);

            if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. " +
                  "Should be formatted as `{iterations}.{salt}.{hash}`");
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var needsUpgrade = iterations != _options.Iterations;

            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              salt,
              iterations,
              HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(KeySize);

                bool verified = keyToCheck.SequenceEqual(key);

                return verified;
            }
        }





    }
}
