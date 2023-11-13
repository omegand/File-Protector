namespace FileProtector;

public class FileOperations
{
    public static readonly string encryptionAppend = ".encr";

    public static void Delete(string path)
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
