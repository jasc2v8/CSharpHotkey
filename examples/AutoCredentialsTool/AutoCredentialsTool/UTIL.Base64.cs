//2022-05-25-1623

using System;

namespace UTIL
{
    public static class Base64
    {
        public static string Encode(string plainText)
        {
            try
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception ex)
            {
                return String.Empty;
            }

        }
        public static string Decode(string base64EncodedData)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }
    }
}
