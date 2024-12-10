using Microsoft.Win32;
using System.Diagnostics;

namespace FileProtector;

internal static class ContextMenuOperations
{
    private const string MenuName = "File Protector";
    private const string CommandStoreBasePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\";
    private static readonly string ProgramPath;
    private static readonly string AssemblyName;
    private static readonly string DirectoryShellPath;

    static ContextMenuOperations()
    {
        ProcessModule process = Process.GetCurrentProcess().MainModule
            ?? throw new InvalidOperationException("Failed to get current process.");

        ProgramPath = process.FileName;
        AssemblyName = Path.GetFileNameWithoutExtension(process.ModuleName);
        DirectoryShellPath = $@"Directory\shell\{AssemblyName}";
    }

    public static int ToggleContextMenu()
    {
        if (!Utility.IsWindowsAdmin())
        {
            return 1;
        }

        if (IsContextMenuRegistered())
        {
            RemoveContextMenu();
        }
        else
        {
            AddContextMenu();
        }

        return 0;
    }

    private static bool IsContextMenuRegistered()
    {
        return Registry.ClassesRoot.OpenSubKey(DirectoryShellPath) != null;
    }

    private static void AddContextMenu()
    {
        try
        {
            RegisterMainContextMenuKey();
            RegisterContextMenuCommands();
            Console.WriteLine("Context menu added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding context menu: {ex.Message}");
        }
    }

    private static void RegisterMainContextMenuKey()
    {
        using RegistryKey key = Registry.ClassesRoot.CreateSubKey(DirectoryShellPath);
        key.SetValue("MUIVerb", MenuName);
        key.SetValue("SubCommands", $"{AssemblyName}.encrypt;{AssemblyName}.decrypt;{AssemblyName}.encrypt_admin;{AssemblyName}.decrypt_admin");
        key.SetValue("Icon", ProgramPath);
    }

    private static void RegisterContextMenuCommands()
    {
        var commands = new[]
        {
            new { Name = "encrypt", Args = "--en -d \"%1\"", Admin = false },
            new { Name = "decrypt", Args = "--de -d \"%1\"", Admin = false },
            new { Name = "encrypt_admin", Args = "--en -d \"%1\"", Admin = true },
            new { Name = "decrypt_admin", Args = "--de -d \"%1\"", Admin = true }
        };

        foreach (var cmd in commands)
        {
            RegisterSingleContextMenuCommand(cmd.Name, cmd.Args, cmd.Admin);
        }
    }

    private static void RegisterSingleContextMenuCommand(string command, string arguments, bool runAsAdmin)
    {
        using RegistryKey baseReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using RegistryKey key = baseReg.CreateSubKey(CommandStoreBasePath + $"{AssemblyName}.{command}");

        string commandName = Utility.FirstCharToUpper(command.Replace("_admin", "")) + (runAsAdmin ? " (Admin)" : "");
        key.SetValue(null, commandName);

        using RegistryKey commandKey = key.CreateSubKey("command");
        commandKey.SetValue(null, $"\"{ProgramPath}\" {arguments}");

        if (runAsAdmin)
        {
            commandKey.SetValue("DelegateExecute", "");
        }
    }

    private static void RemoveContextMenu()
    {
        string[] commandsToRemove =
        [
            "encrypt", "decrypt", "encrypt_admin", "decrypt_admin"
        ];
        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(DirectoryShellPath);

            foreach (string cmd in commandsToRemove)
            {
                Registry.LocalMachine.DeleteSubKeyTree(
                    $"{CommandStoreBasePath}{AssemblyName}.{cmd}",
                    false
                );
            }

            Console.WriteLine("Context menu removed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing context menu: {ex.Message}");
        }
    }
}