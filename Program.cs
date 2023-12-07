using CommandLine;
using System.Diagnostics;

namespace FileProtector;

public class Program
{
    public enum Actions
    {
        Encrypt,
        Decrypt,
        Both
    }

    public static bool SafeMode { get; set; }
    private static Cryptography Crypto { get; set; }
    private static Actions Action { get; set; } = Actions.Both;
    private static Dictionary<bool, string[]> files = new();

    static void Main(string[] args)
    {
        var parserResult = Parser.Default.ParseArguments<Options, ToggleContextMenu, Server>(args);
        parserResult.WithParsed<Options>(RunWithOptions)
                    .WithParsed<ToggleContextMenu>(ctx => ContextMenuOperations.ToggleContextMenu())
                    .WithParsed<Server>(ServerClientOperations.ServerLoop);
        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
        return;
    }

    private static void RunWithOptions(Options options)
    {
        Console.WriteLine("Do not exit the program until it's done, as it might corrupt your files.");
        files = FileOperations.GetFiles(options.Directory, options.Limit);

        if (!ValidateOptions(options))
        {
            return;
        }

        var (IV, aesKey) = Cryptography.GetKeys(options.Password);
        Crypto = new(aesKey, IV);

        Information info = new(files, Action, options.Password);
        if (!Utility.ConfirmAction(info.ToString()))
        {
            Environment.Exit(1);
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();
        ProcessFiles(files);
        sw.Stop();
        Console.WriteLine($"Process took: {sw.Elapsed}");
        Console.WriteLine("Peak Memory Usage: " + (Process.GetCurrentProcess().PeakWorkingSet64 / (1024 * 1024)) + " MB");
        Console.WriteLine("Done.");
    }

    private static bool ValidateOptions(Options options)
    {
        if (options.EncryptOnly && options.DecryptOnly)
        {
            Console.WriteLine("Only specify -e or -d, not both.");
            return false;
        }
        SetAction(options);

        if (!Directory.Exists(options.Directory))
        {
            Console.WriteLine($"Argument: {options.Directory} isn't a directory or doesn't exist, try again.");
            return false;
        }

        if (options.SafeMode)
        {
            SafeMode = options.SafeMode;
            Console.WriteLine("Running in safe mode.");
        }

        PasswordCheck(options);

        while (!PasswordIsValid(options.Password))
        {
            options.Password = "";
            PasswordCheck(options, false);
        }

        ServerClientOperations.StartServer(options.Password);
        return true;
    }

    private static void PasswordCheck(Options options, bool checkServer = true)
    {
        if (string.IsNullOrWhiteSpace(options.Password))
        {
            string password = "";

            if (checkServer)
                password = ServerClientOperations.GetPasswordFromServer();

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

    private static void SetAction(Options options)
    {
        if (options.EncryptOnly) Action = Actions.Encrypt;
        if (options.DecryptOnly) Action = Actions.Decrypt;
    }

    //true = encrypted files, false = regular
    private static void ProcessFiles(Dictionary<bool, string[]> allFiles)
    {
        switch (Action)
        {
            case Actions.Encrypt:
                ParallelAction(allFiles[false]);
                break;
            case Actions.Decrypt:
                ParallelAction(allFiles[true], true);
                break;
            case Actions.Both:
                ParallelAction(allFiles[false]);
                ParallelAction(allFiles[true], true);
                break;
        }
    }

    private static void ParallelAction(string[] files, bool encrypted = false)
    {
        Parallel.ForEach(files, file =>
        {
            if (encrypted)
            {
                Crypto.Decrypt(file);
            }
            else
            {
                Crypto.Encrypt(file);
            }
        });
    }

    private static bool PasswordIsValid(string password)
    {
        if (files[true].Length != 0)
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
