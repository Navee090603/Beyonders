using System;

using System.Security.Cryptography;

using System.Text;

namespace IKart_ServerSide.Helpers

{

    public static class PasswordHelper

    {

        // Hashes the password using SHA256

        public static string HashPassword(string password)

        {

            if (string.IsNullOrEmpty(password))

                throw new ArgumentException("Password cannot be null or empty.");

            using (SHA256 sha256 = SHA256.Create())

            {

                // Convert password string to byte array

                byte[] bytes = Encoding.UTF8.GetBytes(password);

                // Compute hash

                byte[] hashBytes = sha256.ComputeHash(bytes);

                // Convert hash bytes to hexadecimal string

                StringBuilder builder = new StringBuilder();

                foreach (var b in hashBytes)

                {

                    builder.Append(b.ToString("x2")); // hex format

                }

                return builder.ToString();

            }

        }

        // Verifies if a password matches the stored hash

        public static bool VerifyPassword(string password, string storedHash)

        {

            string hashOfInput = HashPassword(password);

            return string.Equals(hashOfInput, storedHash, StringComparison.OrdinalIgnoreCase);

        }

    }

}