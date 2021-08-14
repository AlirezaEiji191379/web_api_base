using Event_Creator.Other.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Event_Creator.Other.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly CaptchaConfig _captcha;

        public CaptchaService(CaptchaConfig captchaConfig)
        {
            _captcha = captchaConfig;
        }

        public async Task<bool> IsCaptchaValid(string token)
        {
            if(token==null)
            {
                return false;
            }
            var googleVerificationUrl = "https://www.google.com/recaptcha/api/siteverify";

            try
            {
                using var client = new HttpClient();
                var httpResponse = await client.GetAsync($"{googleVerificationUrl}?secret={_captcha.SecretKey}&response={token}");
                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return false;
                }
                String jsonResponse = httpResponse.Content.ReadAsStringAsync().Result;
                dynamic jsonData = JObject.Parse(jsonResponse);
                if (jsonData.success != true.ToString().ToLower())
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
