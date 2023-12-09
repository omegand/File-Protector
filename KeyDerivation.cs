using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace FileProtector;

public class KeyDerivation
{
    private const int keySize = 16;
    public static byte[] DeriveKey(byte[] password, int length = keySize)
    {
        Argon2id argon2 = new(password)
        {
            DegreeOfParallelism = GetAvailableCores(),
            MemorySize = Math.Max(47104, ApproximateMaxMemoryUsage() / 2), // https://tobtu.com/minimum-password-settings/
            Iterations = 5 // Don't know how to appropriately calculate iterations yet
        };
        return argon2.GetBytes(length);
    }

    /// <summary>
    /// IV can be public, so might as well use pkdf2 to get 16 bytes
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static byte[] DeriveIV(byte[] password)
    {
        using Rfc2898DeriveBytes pbkdf2 = new(password, GenerateNotRandomSalt(password), 1, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    /// <summary>
    /// Just annoying a potential hacker more than anything
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
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
    /// <returns></returns>
    public static int GetAvailableCores()
    {
        return Math.Max(Environment.ProcessorCount - 4, 1);
    }

    /// <summary>
    /// Approximates the peak memory usage of the application.
    /// </summary>
    /// <returns>The estimated peak memory usage in kilobytes.</returns>
    public static int ApproximateMaxMemoryUsage()
    {
        int bufferSize = Cryptography.bufferSize;
        int processorCount = Environment.ProcessorCount;
        int maxMemoryUsage = bufferSize * processorCount * 3;

        return maxMemoryUsage / 1024;
    }

    public static byte[] ToBytes(string input)
    {
        return Encoding.ASCII.GetBytes(input);
    }
}