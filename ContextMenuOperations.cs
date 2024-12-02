using Microsoft.Win32;
using System.Diagnostics;

namespace FileProtector;

internal static class ContextMenuOperations
{
    private const string MenuName = "File Protector";
    private const string CommandStorePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\";

    private static readonly string DirectoryShellPath = @"Directory\shell\";
    private static readonly string ProgramPath;
    private static readonly string AssemblyName;

    static ContextMenuOperations()
    {
        ProcessModule? executablePath = Process.GetCurrentProcess().MainModule;
        ProgramPath = executablePath.FileName;
        AssemblyName = executablePath.ModuleName[..^4];
        DirectoryShellPath += AssemblyName;
    }

    public static int ToggleContextMenu()
    {
        if (RegistryKeyExists(DirectoryShellPath))
        {
            RemoveContextMenu();
        }
        else
        {
            AddContextMenu();
        }

        return 0;
    }

    private static void AddContextMenu()
    {
        try
        {
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(DirectoryShellPath))
            {
                key.SetValue("MUIVerb", MenuName);
                key.SetValue("SubCommands", $"{AssemblyName}.encrypt;{AssemblyName}.decrypt;{AssemblyName}.encrypt_admin;{AssemblyName}.decrypt_admin");
                key.SetValue("Icon", ProgramPath);
            }

            RegisterContextMenuCommand("encrypt", "--en -d \"%1\"", false);
            RegisterContextMenuCommand("decrypt", "--de -d \"%1\"", false);

            RegisterContextMenuCommand("encrypt_admin", "--en -d \"%1\"", true);
            RegisterContextMenuCommand("decrypt_admin", "--de -d \"%1\"", true);

            Console.WriteLine("Context menu added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error adding context menu: " + ex.Message);
        }
    }

    private static void RegisterContextMenuCommand(string command, string arguments, bool runAsAdmin)
    {
        using RegistryKey baseReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using RegistryKey key = baseReg.CreateSubKey(CommandStorePath + $"{AssemblyName}.{command}");

        // Display name for the command
        string commandName = Utility.FirstCharToUpper(command.Replace("_admin", "")) + (runAsAdmin ? " (Admin)" : "");
        key.SetValue(null, commandName);

        using RegistryKey commandKey = key.CreateSubKey("command");

        // Use the "runas" verb to run the command as administrator if specified
        if (runAsAdmin)
        {
            commandKey.SetValue(null, $"\"{ProgramPath}\" {arguments}");
            commandKey.SetValue("DelegateExecute", "");
        }
        else
        {
            commandKey.SetValue(null, $"\"{ProgramPath}\" {arguments}");
        }
    }

    private static void RemoveContextMenu()
    {
        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(DirectoryShellPath, false);
            Registry.LocalMachine.DeleteSubKeyTree(CommandStorePath + $"{AssemblyName}.encrypt", false);
            Registry.LocalMachine.DeleteSubKeyTree(CommandStorePath + $"{AssemblyName}.decrypt", false);
            Registry.LocalMachine.DeleteSubKeyTree(CommandStorePath + $"{AssemblyName}.encrypt_admin", false);
            Registry.LocalMachine.DeleteSubKeyTree(CommandStorePath + $"{AssemblyName}.decrypt_admin", false);

            Console.WriteLine("Context menu removed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error removing context menu:\n" + ex.Message);
        }
    }

    private static bool RegistryKeyExists(string path)
    {
        try
        {
            using RegistryKey? key = Registry.ClassesRoot.OpenSubKey(path);
            return key != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking registry key: {ex.Message}");
            return false;
        }
    }
}