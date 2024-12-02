using Konscious.Security.Cryptography;
using System.Security.Cryptography;

namespace FileProtector;

/// <summary>
/// Provides methods for securely deriving cryptographic keys and initialization vectors.
/// </summary>
public static class KeyDerivation
{
    private const int keySize = 16;

    /// <summary>
    /// Derives a cryptographic key using the Argon2id key derivation function.
    /// </summary>
    /// <param name="password">The password as a byte array.</param>
    /// <param name="length">The desired key length in bytes. Defaults to 16.</param>
    /// <returns>A derived key as a byte array.</returns>
    public static byte[] DeriveKey(byte[] password, int length = keySize)
    {
        Argon2id argon2 = new(password)
        {
            DegreeOfParallelism = GetAdjustedCoreCount(),
            MemorySize = Math.Max(47104, ApproximateMaxMemoryUsage() / 2), // https://tobtu.com/minimum-password-settings/
            Iterations = 5
        };
        return argon2.GetBytes(length);
    }

    /// <summary>
    /// Derives a 16-byte (IV) using PBKDF2. (Weak encryption)
    /// </summary>
    /// <param name="password">The password as a byte array.</param>
    /// <returns>A derived IV as a byte array.</returns>
    public static byte[] DeriveIV(byte[] password)
    {
        using Rfc2898DeriveBytes pbkdf2 = new(password, GenerateNotRandomSalt(password), 1, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    /// <summary>
    /// Generates a pseudo-random salt based on the password, adding an extra layer of difficulty for attackers.
    /// </summary>
    /// <param name="password">The password as a byte array.</param>
    /// <returns>A pseudo-randomly generated salt as a byte array.</returns>
    private static byte[] GenerateNotRandomSalt(byte[] password)
    {
        Random rnd = new(password.Sum(item => item));
        byte[] salt = new byte[16];
        rnd.NextBytes(salt);
        return salt;
    }

    /// <summary>
    /// Returns available cores - 4 or 1, if not enough cores
    /// </summary>
    /// <returns>The number of available cores for parallelism.</returns>
    public static int GetAdjustedCoreCount()
    {
        return Math.Max(Environment.ProcessorCount - 4, 1);
    }

    /// <summary>
    /// Approximates the peak memory usage of the application.
    /// </summary>
    /// <returns>The estimated peak memory usage in kilobytes.</returns>
    public static int ApproximateMaxMemoryUsage()
    {
        const int bufferSize = Cryptography.bufferSize;
        int processorCount = Environment.ProcessorCount;
        int maxMemoryUsage = bufferSize * processorCount * 3;

        return maxMemoryUsage / 1024;
    }
}