using FileProtector.Enums;
using System.Text;

namespace FileProtector;

public static class FileOperations
{
    public static readonly string encryptionAppend = ".encr";
    private static readonly Dictionary<char, char> replacements = new()
    {
        { '+', '-' },
        { '/', '_' }
    };
    public static void Delete(string path)
    {
        if (Program.SafeMode)
        {
            return;
        }

        File.Delete(path);
    }

    /// <summary>
    /// Retrieves encrypted and non-encrypted FilesToProcess from the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search for FilesToProcess.</param>
    /// <param name="limit">The maximum number of FilesToProcess to retrieve. Use 0 for no limit, a positive value for a limit from the start, and a negative value for a limit from the end.</param>
    /// <returns>A dictionary where the key represents whether the FilesToProcess are encrypted (true) or not (false), and the value is an array of corresponding file paths.</returns>
    public static Dictionary<bool, string[]> GetFiles(string path, int limit, ProcessActions action)
    {
        string[] allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        return action switch
        {
            ProcessActions.Encrypt => new Dictionary<bool, string[]>
            {
                [true] = [],
                [false] = GetRegularFiles(allFiles, limit)
            },
            ProcessActions.Decrypt => new Dictionary<bool, string[]>
            {
                [true] = GetEncryptedFiles(allFiles, limit),
                [false] = []
            },
            ProcessActions.Both => new Dictionary<bool, string[]>
            {
                [true] = GetEncryptedFiles(allFiles, limit),
                [false] = GetRegularFiles(allFiles, limit)
            },
            _ => throw new ArgumentException("Invalid action argument.")
        };
    }

    private static string[] GetEncryptedFiles(string[] allFiles, int limit)
    {
        string[] encryptedFiles = allFiles.Where(file => file.EndsWith(encryptionAppend, StringComparison.OrdinalIgnoreCase)).ToArray();
        return ApplyLimit(encryptedFiles, limit);
    }

    private static string[] GetRegularFiles(string[] allFiles, int limit)
    {
        string[] regularFiles = allFiles.Where(file => !file.EndsWith(encryptionAppend, StringComparison.OrdinalIgnoreCase)).ToArray();
        return ApplyLimit(regularFiles, limit);
    }

    private static string[] ApplyLimit(string[] files, int limit)
    {
        return limit switch
        {
            0 => files,
            > 0 => files.Take(limit).ToArray(),
            < 0 => files.TakeLast(Math.Abs(limit)).ToArray(),
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

    public static void SetFileDates(string newFile, string oldFile)
    {
        try
        {
            File.SetCreationTimeUtc(newFile, File.GetCreationTimeUtc(oldFile));
            File.SetLastWriteTimeUtc(newFile, File.GetLastWriteTimeUtc(oldFile));
        }
        catch (Exception)
        {
            Console.WriteLine("A problem occured trying to set file dates, skipping.");
        }
    }

    public static bool Empty(IDictionary<bool, string[]> files)
    {
        return files[true].Length == 0 && files[false].Length == 0;
    }

    public static string FixFaultyFileName(string name)
    {
        return ReplaceCharacters(name, replacements);
    }

    public static string RestoreFaultyName(string name)
    {
        return ReplaceCharacters(name, replacements.ToDictionary(kvp => kvp.Value, kvp => kvp.Key));
    }

    private static string ReplaceCharacters(string input, Dictionary<char, char> replacements)
    {
        StringBuilder sb = new(input.Length);
        foreach (char ch in input)
        {
            _ = sb.Append(replacements.TryGetValue(ch, out char replacement) ? replacement : ch);
        }
        return sb.ToString();
    }
}
