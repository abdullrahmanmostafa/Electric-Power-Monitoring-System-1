using System.Security.Cryptography;
using System.Text;

namespace Electric_Power_Monitoring_System.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            var hashOfInput = HashPassword(plainPassword);
            return hashOfInput == hashedPassword;
        }
    }
}