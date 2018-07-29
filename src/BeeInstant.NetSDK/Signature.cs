using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BeeInstant.NetSDK
{
    public static  class Signature
    {
        public static string Sign(byte[] data, string key)
        {
            var keyByteArray = Encoding.UTF8.GetBytes(key);

            var alg = new HMACSHA256(keyByteArray);
            alg.Initialize();

            using (var ms = new MemoryStream(data))
            {
                var hash = alg.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }
    }
}