using System;
using System.Security.Cryptography;
using System.Text;

namespace PastebinApp
{
    public static class HashHelper
    {
        public static string GenerateSha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                string result = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return result;
            }
        }
    }
}