using System;

using System.Security.Cryptography;

using System.Text;

namespace IKart_Shared.Utilities

{

    public static class PasswordHasher

    {

        public static string HashPassword(string password)

        {

            if (string.IsNullOrEmpty(password))

                throw new ArgumentException("Password cannot be null or empty.");

            using (SHA256 sha256 = SHA256.Create())

            {

                byte[] bytes = Encoding.UTF8.GetBytes(password);


                byte[] hashBytes = sha256.ComputeHash(bytes);


                StringBuilder builder = new StringBuilder();

                foreach (var b in hashBytes)

                {

                    builder.Append(b.ToString("x2")); 

                }

                return builder.ToString();

            }

        }

        // Verifies if the provided password matches the stored hash

        public static bool VerifyPassword(string password, string storedHash)

        {

            string hashOfInput = HashPassword(password);

            // String comparison in a case-insensitive manner

            return string.Equals(hashOfInput, storedHash, StringComparison.OrdinalIgnoreCase);

        }

    }

}