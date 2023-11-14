﻿using CommandLine;
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
    public static string CurrentPassword { get; set; }
    private static Cryptography Crypto { get; set; }
    private static Actions Action { get; set; } = Actions.Both;

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options, Reset>(args)
            .WithParsed<Options>(RunWithOptions)
            .WithParsed<Reset>(_ => RunWithReset());
    }

    private static void RunWithOptions(Options options)
    {
        Console.WriteLine("Do not exit the program until it's done, as it might corrupt your files.");
        if (!ValidateOptions(options))
        {
            return;
        }
        SetAction(options);
        CurrentPassword = options.Password;
        byte[] savedPassword = PasswordManager.GetPassword();
        byte[]? aesKey = PasswordManager.VerifyPassword(CurrentPassword, savedPassword);

        if (aesKey == null)
        {
            Console.WriteLine("Password does not match.");
            return;
        }

        byte[] IV = KeyDerivation.DeriveIV(aesKey);
        Crypto = new(aesKey, IV);

        Dictionary<bool, string[]> files = FileOperations.GetFiles(options.Directory);
        Information info = new(files, Action);

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

        return true;
    }

    private static void SetAction(Options options)
    {
        if (options.EncryptOnly) Action = Actions.Encrypt;
        if (options.DecryptOnly) Action = Actions.Decrypt;
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
    private static void RunWithReset()
    {
        if (!Utility.ConfirmAction("You are about to delete your password, are you sure?"))
        {
            Environment.Exit(1);
            return;
        }
        RegistryOperations.DeletePasswordFromRegistry();
    }
}
