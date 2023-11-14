using System.Security.Cryptography;

namespace FileProtector;

public class Cryptography
{
    private readonly Aes aes;
    public const int bufferSize = 1024 * 1024 * 16; // From my research, seems to be a good number for performance and ram-usage
    public Cryptography(byte[] aesKey, byte[] IV)
    {
        aes = Aes.Create();
        aes.IV = IV;
        aes.Key = aesKey;
    }

    public void Encrypt(string file)
    {
        if (!FileOperations.ValidateFile(file)) return;

        try
        {
            using (var encryptor = aes.CreateEncryptor())
            {
                using var inputStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
                using var outputStream = new FileStream(file + FileOperations.encryptionAppend, FileMode.Create, FileAccess.Write);
                using var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
                byte[] buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = inputStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    cryptoStream.Write(buffer, 0, bytesRead);
                }
            }
            FileOperations.Delete(file);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to encrypt: {file} \nError: {ex}");
        }
    }

    public void Decrypt(string file)
    {
        if (!FileOperations.ValidateFile(file)) return;

        try
        {
            using (var decryptor = aes.CreateDecryptor())
            {
                using var inputStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
                using var outputStream = new FileStream(Path.ChangeExtension(file, null), FileMode.Create, FileAccess.Write);
                using CryptoStream cryptoStream = new(outputStream, decryptor, CryptoStreamMode.Write);
                byte[] buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = inputStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    cryptoStream.Write(buffer, 0, bytesRead);
                }
            }
            FileOperations.Delete(file);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to decrypt: {file} \nError: {ex}");
        }
    }

}
