namespace FileProtector;

public class FileOperations
{
    public static readonly string encryptionAppend = ".encr";

    public static void Delete(string path)
    {
        if (Program.SafeMode)
        {
            return;
        }

        File.Delete(path);
    }

    /// <summary>
    /// Retrieves encrypted and non-encrypted files from the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search for files.</param>
    /// <param name="limit">The maximum number of files to retrieve. Use 0 for no limit, a positive value for a limit from the start, and a negative value for a limit from the end.</param>
    /// <returns>A dictionary where the key represents whether the files are encrypted (true) or not (false), and the value is an array of corresponding file paths.</returns>
    public static Dictionary<bool, string[]> GetFiles(string path, int limit, Actions action)
    {
        string[] allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        if (limit != 0)
        {
            allFiles = (limit > 0) ? allFiles.Take(limit).ToArray() : allFiles.TakeLast(Math.Abs(limit)).ToArray();
        }

        return action switch
        {
            Actions.Encrypt => new Dictionary<bool, string[]> { [true] = Array.Empty<string>(), [false] = GetRegularFiles(allFiles) },
            Actions.Decrypt => new Dictionary<bool, string[]> { [true] = GetEncryptedFiles(allFiles), [false] = Array.Empty<string>() },
            Actions.Both => new Dictionary<bool, string[]>
            {
                [true] = GetEncryptedFiles(allFiles),
                [false] = GetRegularFiles(allFiles)
            },
            _ => throw new Exception("Problem with action argument.")
        };
    }


    public static bool ValidateFile(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine($"File does not exist: {file}");
            return false;
        }
        return true;
    }

    public static bool Empty(Dictionary<bool, string[]> files)
    {
        return files[true].Length == 0 && files[false].Length == 0;
    }
    private static string[] GetEncryptedFiles(string[] allFiles)
    {
        return allFiles.Where(file => file.EndsWith(encryptionAppend, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private static string[] GetRegularFiles(string[] allFiles)
    {
        return allFiles.Where(file => !file.EndsWith(encryptionAppend, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}
