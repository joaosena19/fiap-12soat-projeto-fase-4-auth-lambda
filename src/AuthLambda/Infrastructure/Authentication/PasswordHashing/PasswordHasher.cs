using Konscious.Security.Cryptography;
using System.Text;

namespace Infrastructure.Authentication.PasswordHashing
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly Argon2HashingOptions _options;

        public PasswordHasher() : this(new Argon2HashingOptions()) { }

        public PasswordHasher(Argon2HashingOptions options)
        {
            _options = options;
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                var combinedBytes = Convert.FromBase64String(hash);
                if (combinedBytes.Length != _options.SaltSize + _options.HashSize)
                    return false;

                // Extrair salt e hash
                var salt = new byte[_options.SaltSize];
                var originalHash = new byte[_options.HashSize];
                Buffer.BlockCopy(combinedBytes, 0, salt, 0, _options.SaltSize);
                Buffer.BlockCopy(combinedBytes, _options.SaltSize, originalHash, 0, _options.HashSize);

                // Recriar hash com a mesma senha e salt
                var newHash = HashPassword(password, salt);

                // Comparar hashes de forma segura (constant-time)
                return ConstantTimeEquals(originalHash, newHash);
            }
            catch
            {
                return false;
            }
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _options.DegreeOfParallelism,
                Iterations = _options.Iterations,
                MemorySize = _options.MemorySize
            };

            return argon2.GetBytes(_options.HashSize);
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            var result = 0;
            for (var i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}