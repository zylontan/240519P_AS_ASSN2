using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace _240519P_AS_ASSN2.Security
{
    public static class PasswordService
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);

            return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('|');
            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = parts[1];

            byte[] hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);

            return Convert.ToBase64String(hash) == expectedHash;
        }
    }
}
