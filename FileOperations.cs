namespace FileProtector;

public class FileOperations
{
    public static readonly string encryptionAppend = ".encr";

    /// <summary>
    /// Reads bytes from passed path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static byte[] ReadFile(string path)
    {
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Adds encryption extension to path and writes data, deletes passed path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public static void WriteEncrypted(string path, byte[] data)
    {
        if (data.Length == 0) return;
        File.WriteAllBytes(path + encryptionAppend, data);
        Delete(path);
    }

    /// <summary>
    /// Removes last extension from path, saves the data into it, deletes passed path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public static void WriteDecrypted(string path, byte[] data)
    {
        if (data.Length == 0) return;
        string newPath = Path.ChangeExtension(path, null);
        File.WriteAllBytes(newPath, data);
        Delete(path);
    }

    private static void Delete(string path)
    {
        if (Program.SafeMode) return;
        File.Delete(path);
    }

    public static Dictionary<bool, string[]> GetFiles(string path)
    {
        string[] allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        string[] encryptedFiles = allFiles
            .Where(file => file.EndsWith(encryptionAppend, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        string[] nonEncryptedFiles = allFiles
            .Where(file => !file.EndsWith(encryptionAppend, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return new Dictionary<bool, string[]> { { true, encryptedFiles }, { false, nonEncryptedFiles } };
    }

}
