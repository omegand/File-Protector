using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace FileProtector;

public class KeyDerivation
{
    private const int keySize = 16;
    public static byte[] DeriveAesKey(string password)
    {
        Argon2id argon2 = new(ToBytes(password))
        {
            DegreeOfParallelism = GetAvailableCores(),
            MemorySize = 4710, //https://tobtu.com/minimum-password-settings/
            Iterations = 5,
            Salt = GenerateNotRandomSalt(password)
        };
        return argon2.GetBytes(keySize);
    }

    /// <summary>
    /// IV can be public, so might as well use pkdf2 to get 16 bytes
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static byte[] DeriveIV(string password)
    {
        using Rfc2898DeriveBytes pbkdf2 = new(password, GenerateNotRandomSalt(password), 1, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    /// <summary>
    /// Just annoying a potential hacker more than anything
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    private static byte[] GenerateNotRandomSalt(string password)
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
    public static byte[] ToBytes(string input) => Encoding.ASCII.GetBytes(input);

}
