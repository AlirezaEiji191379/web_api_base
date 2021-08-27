using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public static class TypeConverterExtension
    {
        public static byte[] ToByteArray(this string value) =>
               Convert.FromBase64String(value);
    }

    public class JwtConfig
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
