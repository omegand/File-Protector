using System.Security.Cryptography;
using System.Text;

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

    public void EncryptFile(string filePath)
    {
        if (!FileOperations.ValidateFile(filePath))
        {
            return;
        }

        try
        {

            string fileName = Path.GetFileName(filePath);
            string encryptedName = FileOperations.FixFaultyFileName(EncryptFileName(fileName, aes)) + FileOperations.encryptionAppend;
            string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), encryptedName);

            if (Program.IgnoreNames)
            {
                newFilePath = filePath + FileOperations.encryptionAppend;
            }

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                using FileStream inputStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
                using FileStream outputStream = new(newFilePath, FileMode.Create, FileAccess.Write);
                using CryptoStream cryptoStream = new(outputStream, encryptor, CryptoStreamMode.Write);
                byte[] buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = inputStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    cryptoStream.Write(buffer, 0, bytesRead);
                }
            }

            FileOperations.SetFileDates(newFilePath, filePath);
            FileOperations.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to encrypt: {filePath} \nError: {ex}");
        }
    }

    public void DecryptFile(string filePath)
    {
        if (!FileOperations.ValidateFile(filePath))
        {
            return;
        }

        try
        {

            string newFilePath = Path.ChangeExtension(filePath, null);

            if (!Program.IgnoreNames)
            {
                string fileName = Path.ChangeExtension(Path.GetFileName(filePath), null);
                string decryptedName = DecryptFileName(FileOperations.RestoreFaultyName(fileName), aes);
                newFilePath = Path.Combine(Path.GetDirectoryName(filePath), decryptedName);
            }

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                using FileStream inputStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
                using FileStream outputStream = new(newFilePath, FileMode.Create, FileAccess.Write);
                using CryptoStream cryptoStream = new(outputStream, decryptor, CryptoStreamMode.Write);
                byte[] buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = inputStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    cryptoStream.Write(buffer, 0, bytesRead);
                }
            }

            FileOperations.SetFileDates(newFilePath, filePath);
            FileOperations.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to decrypt: {filePath} \nError: {ex}");
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

    public static string EncryptFileName(string fileName, Aes aes)
    {
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] bytes = Encoding.UTF8.GetBytes(fileName);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string DecryptFileName(string encryptedFileName, Aes aes)
    {
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] encryptedBytes = Convert.FromBase64String(encryptedFileName);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

}
