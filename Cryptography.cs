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
        if (!FileOperations.ValidateFile(file))
        {
            return;
        }

        try
        {
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                using FileStream inputStream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
                using FileStream outputStream = new(file + FileOperations.encryptionAppend, FileMode.Create, FileAccess.Write);
                using CryptoStream cryptoStream = new(outputStream, encryptor, CryptoStreamMode.Write);
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
        if (!FileOperations.ValidateFile(file))
        {
            return;
        }

        try
        {
            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                using FileStream inputStream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
                using FileStream outputStream = new(Path.ChangeExtension(file, null), FileMode.Create, FileAccess.Write);
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

    public static bool TestDecryption(string file, string password)
    {
        if (!FileOperations.ValidateFile(file))
        {
            return false;
        }

        try
        {
            Aes aes = Aes.Create();
            (byte[] IV, byte[] aesKey) = GetKeys(password);
            using ICryptoTransform decryptor = aes.CreateDecryptor(aesKey, IV);
            using FileStream inputStream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
            using CryptoStream cryptoStream = new(inputStream, decryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = cryptoStream.Read(buffer, 0, bufferSize)) > 0)
            {
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static (byte[] IV, byte[] Key) GetKeys(string password)
    {
        byte[] aesKey = KeyDerivation.DeriveKey(Utility.ToBytes(password));
        byte[] IV = KeyDerivation.DeriveIV(aesKey);
        return (IV, aesKey);
    }
}
