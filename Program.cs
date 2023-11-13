using CommandLine;
using System.Diagnostics;

namespace FileProtector;

public class Program
{
    public static bool SafeMode { get; set; }
    private static Cryptography Crypto { get; set; }
    private static Actions Action { get; set; } = Actions.Both;

    public enum Actions
    {
        Encrypt,
        Decrypt,
        Both
    }


    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options, Reset>(args)
            .WithParsed<Options>(o => { Main(o); })
            .WithParsed<Reset>(r =>
            {
                RegistryOperations.DeletePasswordFromRegistry();
            });
    }
    private static void Main(Options o)
    {
        Process currentProcess = Process.GetCurrentProcess();
        Console.WriteLine("Do not exit the program until it's done, as it might corrupt your files.");
        if (o.EncryptOnly && o.DecryptOnly)
        {
            Console.WriteLine("Only specify -e or -d, not both.");
            return;
        }
        if (!Directory.Exists(o.Directory))
        {
            Console.WriteLine($"Argument: {o.Directory} isn't a directory or doesn't exist, try again.");
            return;
        }
        if (o.SafeMode)
        {
            SafeMode = o.SafeMode;
            Console.WriteLine("Running in safe mode.");
        }

        if (o.EncryptOnly) Action = Actions.Encrypt;
        if (o.DecryptOnly) Action = Actions.Decrypt;
        Crypto = new(o.Password);
        CheckPassword();
        var files = FileOperations.GetFiles(o.Directory);
        _ = new Information(files, Action);
        if (!ConfirmAction())
        {
            Environment.Exit(1);
            return;
        }
        Stopwatch sw = Stopwatch.StartNew();
        ProcessFiles(files);
        sw.Stop();
        Console.WriteLine($"Process took: {sw.Elapsed}");
        Console.WriteLine("Peak Memory Usage: " + (currentProcess.PeakWorkingSet64 / (1024 * 1024)) + " MB");
        Console.WriteLine("Done.");
    }

    //allFiles, true = encrypted files, false = regular
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

    private static void CheckPassword()
    {
        var existingKey = RegistryOperations.GetPassword();
        if (existingKey == null)
        {
            Console.WriteLine("Password not found.");
            CreatePassword();
        }
        else
        {
            if (existingKey.ToString() != Crypto.GetAesKey())
            {
                Console.WriteLine("Password is incorrect.");
                Environment.Exit(1);
            }
            Console.WriteLine("Password is correct.");
        }
    }

    private static void CreatePassword()
    {
        Console.WriteLine("Creating a new password... (If you have already created a password for this PC, use the same one again)");
        string[] passwords = new string[3];
        passwords[0] = Utility.GetInput("Enter your password: ");
        passwords[1] = Utility.GetInput("Repeat your password: ");
        passwords[2] = Utility.GetInput("Repeat your password again: ");


        if (passwords.All(p => p == passwords[0]))
        {
            RegistryOperations.SaveIntoRegistry(Crypto.GetAesKey());
            Console.WriteLine("\nPassword created successfully!");
            Console.WriteLine(passwords[0]);
        }
        else
        {
            Console.WriteLine("\nPasswords do not match. Please try again.");
            CreatePassword();
        }
    }

    private static bool ConfirmAction()
    {
        Console.WriteLine("Press 'A' to [A]ccept or 'C' to [C]ancel");

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.A:
                    Console.WriteLine("\nAction accepted!");
                    return true;

                case ConsoleKey.C:
                    Console.WriteLine("\nAction canceled.");
                    return false;

                default:
                    Console.WriteLine("\nInvalid input. Press 'A' to accept or 'C' to cancel");
                    break;
            }
        }
    }

}
