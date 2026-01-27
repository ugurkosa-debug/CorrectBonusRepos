using System.Security.Cryptography;
using System.Text;

namespace CorrectBonus.Services.Security
{
    public static class PasswordHasher
    {
        public static void CreateHash(
            string password,
            out byte[] passwordHash,
            out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public static bool Verify(
            string password,
            byte[] storedHash,
            byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
    }
}
