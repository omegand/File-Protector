using System.Security.Cryptography;

namespace FileProtector;

public class Cryptography
{
    private readonly Aes aes;
    private const int bufferSize = 1024 * 1024 * 16; // From my research, seems to be a good number for performance and ram-usage
    public Cryptography(string password)
    {
        aes = Aes.Create();
        aes.IV = KeyDerivation.DeriveIV(password);
        aes.Key = KeyDerivation.DeriveAesKey(password);
    }

    public string GetAesKey() => Convert.ToBase64String(aes.Key);
    public void Encrypt(string file)
    {
        if (!ValidateFile(file)) return;

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
        if (!ValidateFile(file)) return;

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

    private static bool ValidateFile(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine($"File does not exist: {file}");
            return false;
        }
        return true;
    }

}
