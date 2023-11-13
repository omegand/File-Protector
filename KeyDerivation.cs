using System.Security.Cryptography;

namespace FileProtector;

public class KeyDerivation
{
    private const int iterations = 6969696;
    private const int keySize = 16;
    public static byte[] DeriveAesKey(string password)
    {
        using Rfc2898DeriveBytes pbkdf2 = new(password, GenerateRandomSalt(CalculateSeed(password)), iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    public static byte[] DeriveIV(string password)
    {
        using Rfc2898DeriveBytes pbkdf2 = new(password, GenerateRandomSalt(CalculateSeed(password) + iterations), 1, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    private static int CalculateSeed(string password)
    {
        return password.Sum(item => item);
    }

    private static byte[] GenerateRandomSalt(int seed)
    {
        Random rnd = new(seed);
        byte[] salt = new byte[16];
        rnd.NextBytes(salt);
        return salt;
    }
}
