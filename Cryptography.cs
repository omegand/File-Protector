using System.Security.Cryptography;

namespace FileProtector;

public class Cryptography
{
    private readonly Aes aes;
    public Cryptography(string password)
    {
        aes = Aes.Create();
        aes.IV = KeyDerivation.DeriveIV(password);
        aes.Key = KeyDerivation.DeriveAesKey(password);
    }

    public string GetAesKey() => Convert.ToBase64String(aes.Key);
    public byte[] Encrypt(byte[] bytes)
    {
        try
        {
            using ICryptoTransform encryptor = aes.CreateEncryptor(); //ICryptoTransform is not thread-safe
            using MemoryStream memoryStream = new(bytes.Length);
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to encrypt: \nError: {ex}");
            return Array.Empty<byte>();
        }
    }

    public byte[] Decrypt(byte[] bytes)
    {
        try
        {
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using MemoryStream memoryStream = new(bytes.Length);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to decrypt: \nError: {ex}");
            return Array.Empty<byte>();
        }
    }

}
