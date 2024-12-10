using CommandLine;
using FileProtector.CommandLineOptions;
using FileProtector.Enums;
using System.Diagnostics;

namespace FileProtector;
public static class Program
{
    public static bool SafeMode { get; set; }
    public static bool IgnoreNames { get; set; }
    private static Cryptography? Crypto { get; set; }
    private static ProcessActions Action { get; set; } = ProcessActions.Both;
    private static Dictionary<bool, string[]> FilesToProcess { get; set; } = [];

    private static void Main(string[] args)
    {
        if (!Utility.IsWindows())
        {
            Console.WriteLine("Application only supported for windows.");
            return;
        }
        ParserResult<object> parserResult = Parser.Default.ParseArguments<FileProcessingOptions, ContextMenuToggleOptions, InternalServerOptions>(args);

        _ = parserResult.MapResult(
            (FileProcessingOptions opts) => RunWithOptions(opts),
            (ContextMenuToggleOptions opts) => ContextMenuOperations.ToggleContextMenu(),
            (InternalServerOptions opts) => PasswordStorage.StorePassword(opts),
            _ => 1
            );

        Console.WriteLine("Finished.");
        _ = Console.ReadKey();
    }

    private static int RunWithOptions(FileProcessingOptions options)
    {
        CheckArguments(options);
        CheckFiles(options);
        CheckPasswordArgument(options);
        PasswordStorage.StartServer(options.Password);

        (byte[] IV, byte[] aesKey) = Cryptography.GetKeys(options.Password);
        Crypto = new(aesKey, IV);

        Information info = new(FilesToProcess, options.Password);
        if (!Utility.ConfirmAction(info.ToString()))
        {
            Utility.ExitWithInput(6);
        }

        Stopwatch sw = Stopwatch.StartNew();
        ProcessFiles(FilesToProcess);
        sw.Stop();
        Console.WriteLine($"Process took: {sw.Elapsed}");
        Console.WriteLine($"Peak Memory Usage: {Process.GetCurrentProcess().PeakWorkingSet64 / (1024 * 1024)} MB");

        return 0;
    }

    private static void CheckFiles(FileProcessingOptions options)
    {
        FilesToProcess = FileOperations.GetFiles(options.Directory, options.Limit, Action);
        if (FileOperations.Empty(FilesToProcess))
        {
            Console.WriteLine($"Did not find any FilesToProcess.\n{options.Directory}");
            Utility.ExitWithInput(3);
        }
    }

    private static void CheckPasswordArgument(FileProcessingOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Password))
        {
            options.Password = PasswordStorage.GetPasswordFromServer();
        }

        while (!PasswordIsValid(options.Password))
        {
            options.Password = Utility.GetInput("Enter your password:");
        }
    }

    private static void CheckArguments(FileProcessingOptions options)
    {
        if (options.EncryptOnly && options.DecryptOnly)
        {
            Console.WriteLine("Only specify -e or -d, not both.");
            Utility.ExitWithInput(9);
        }

        if (!Directory.Exists(options.Directory))
        {
            Console.WriteLine($"Argument: {options.Directory} isn't a directory or doesn't exist, try again.");
            Utility.ExitWithInput(9);
        }

        if (options.EncryptOnly)
        {
            Action = ProcessActions.Encrypt;
        }

        if (options.DecryptOnly)
        {
            Action = ProcessActions.Decrypt;
        }

        if (options.SafeMode)
        {
            SafeMode = options.SafeMode;
            Console.WriteLine("Running in safe mode.");
        }

        if (options.IgnoreNames)
        {
            IgnoreNames = options.IgnoreNames;
            Console.WriteLine("Ignoring file names during encryption.");
        }
    }

    //true = encrypted FilesToProcess, false = regular
    private static void ProcessFiles(Dictionary<bool, string[]> allFiles)
    {
        if (Crypto is null)
        {
            Console.WriteLine("Failed to instantiate Cryptography class, cancelling process.");
            return;
        }

        if (allFiles.TryGetValue(true, out string[]? encryptedFiles))
        {
            ParallelProcessFiles(encryptedFiles, Crypto.DecryptFile);
        }
        if (allFiles.TryGetValue(false, out string[]? regularFiles))
        {
            ParallelProcessFiles(regularFiles, Crypto.EncryptFile);
        }
    }

    private static void ParallelProcessFiles(string[] files, Action<string> fileProcessingAction)
    {
        _ = Parallel.ForEach(files, file => fileProcessingAction(file));
    }

    private static bool PasswordIsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (FilesToProcess.TryGetValue(true, out string[]? files) && files.Length > 0)
        {
            bool isPasswordValid = Cryptography.TestDecryption(files[0], password);
            Console.WriteLine(isPasswordValid ? "Password is correct." : "Password is not valid.");

            return isPasswordValid;
        }

        return true;
    }
}
