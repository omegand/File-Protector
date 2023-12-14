using CommandLine;
using System.Diagnostics;

namespace FileProtector;
//Error codes:
//9 - Problem with arguments
//6 - User based exit  
//3 - No files found
//4 - Problem with OS permissions
//5 - Unsupported OS
public enum Actions
{
    Encrypt,
    Decrypt,
    Both
}

public class Program
{
    public static bool SafeMode;
    private static Cryptography? Crypto;
    private static Actions Action = Actions.Both;
    private static Dictionary<bool, string[]> files = new();

    private static void Main(string[] args)
    {
        ParserResult<object> parserResult = Parser.Default.ParseArguments<Options, ToggleContextMenu, Server>(args);
        _ = parserResult.WithParsed<Options>(RunWithOptions)
                    .WithParsed<ToggleContextMenu>(ctx => ContextMenuOperations.ToggleContextMenu())
                    .WithParsed<Server>(ServerClientOperations.ServerLoop);
        Console.WriteLine("Press any key to close...");
        _ = Console.ReadKey();
        return;
    }

    private static void RunWithOptions(Options options)
    {
        Console.WriteLine("Do not exit the program until it's done, as it might corrupt your files.");
        CheckArguments(options);
        CheckFiles(options);
        CheckPasswordArgument(options);
        ServerClientOperations.StartServer(options.Password);

        (byte[] IV, byte[] aesKey) = Cryptography.GetKeys(options.Password);
        Crypto = new(aesKey, IV);

        Information info = new(files, options.Password);
        if (!Utility.ConfirmAction(info.ToString()))
        {
            Utility.ExitWithInput(6);
        }

        Stopwatch sw = Stopwatch.StartNew();
        ProcessFiles(files);
        sw.Stop();
        Console.WriteLine($"Process took: {sw.Elapsed}");
        Console.WriteLine("Peak Memory Usage: " + (Process.GetCurrentProcess().PeakWorkingSet64 / (1024 * 1024)) + " MB");
        Console.WriteLine("Done.");
    }

    private static void CheckFiles(Options options)
    {
        files = FileOperations.GetFiles(options.Directory, options.Limit, Action);
        if (FileOperations.Empty(files))
        {
            Console.WriteLine($"Did not find any files.\n{options.Directory}");
            Utility.ExitWithInput(3);
        }
    }

    private static void CheckPasswordArgument(Options options)
    {
        TryGetPassword(options);

        while (!PasswordIsValid(options.Password))
        {
            options.Password = "";
            TryGetPassword(options, false);
        }
    }

    private static void CheckArguments(Options options)
    {
        if (options.EncryptOnly && options.DecryptOnly)
        {
            Console.WriteLine("Only specify -e or -d, not both.");
            Utility.ExitWithInput(9);
        }

        if (options.EncryptOnly)
        {
            Action = Actions.Encrypt;
        }

        if (options.DecryptOnly)
        {
            Action = Actions.Decrypt;
        }

        if (!Directory.Exists(options.Directory))
        {
            Console.WriteLine($"Argument: {options.Directory} isn't a directory or doesn't exist, try again.");
            Utility.ExitWithInput(9);
        }

        if (options.SafeMode)
        {
            SafeMode = options.SafeMode;
            Console.WriteLine("Running in safe mode.");
        }
    }

    private static void TryGetPassword(Options options, bool checkServer = true)
    {
        if (string.IsNullOrWhiteSpace(options.Password))
        {
            string password = "";

            if (checkServer)
            {
                password = ServerClientOperations.GetPasswordFromServer();
            }

            if (password == "")
            {
                Console.WriteLine("Password server doesn't exist or is incorrect.");
                password = InputPassword();
            }
            options.Password = password;
        }
    }

    private static string InputPassword()
    {
        string password = "";
        while (string.IsNullOrWhiteSpace(password))
        {
            password = Utility.GetInput("Enter your password:");
        }
        return password;
    }

    //true = encrypted files, false = regular
    private static void ProcessFiles(Dictionary<bool, string[]> allFiles)
    {
        if (allFiles.TryGetValue(true, out string[]? encryptedFiles))
        {
            ParallelProcessFiles(encryptedFiles, Crypto.Decrypt);
        }
        if (allFiles.TryGetValue(false, out string[]? regularFiles))
        {
            ParallelProcessFiles(regularFiles, Crypto.Encrypt);
        }
    }

    private static void ParallelProcessFiles(string[] files, Action<string> fileProcessingAction)
    {
        _ = Parallel.ForEach(files, file =>
        {
            fileProcessingAction(file);
        });
    }

    private static bool PasswordIsValid(string password)
    {
        if (files.ContainsKey(true) && files[true].Length > 0)
        {
            Console.WriteLine("Checking if password is valid...");
            if (!Cryptography.TestDecryption(files[true][0], password))
            {
                Console.WriteLine("Password is not valid.");
                return false;
            }
            Console.WriteLine("Password is correct.");
        }

        return true;
    }

}
