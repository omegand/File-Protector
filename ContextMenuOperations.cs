using Microsoft.Win32;
using System.Diagnostics;

namespace FileProtector;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "CONSTRUCTOR CHECKS FOR WINDOWS")]
internal class ContextMenuOperations
{
    private const string MenuName = "File Protector";
    private const string CommandStorePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\";

    private static string DirectoryShellPath = @"Directory\shell\";
    private static string ProgramPath;
    private static string AssemblyName;

    static ContextMenuOperations()
    {
        Utility.VerifyWindows();
        Utility.VerifyAdmin();

        var exePath = Process.GetCurrentProcess().MainModule;
        ProgramPath = exePath.FileName;
        AssemblyName = exePath.ModuleName[..^4];
        DirectoryShellPath += AssemblyName;
    }

    public static void ToggleContextMenu()
    {
        if (RegistryKeyExists(DirectoryShellPath))
        {
            RemoveContextMenu();
        }
        else
        {
            AddContextMenu();
        }
    }

    private static void AddContextMenu()
    {
        try
        {
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(DirectoryShellPath))
            {
                key.SetValue("MUIVerb", MenuName);
                key.SetValue("SubCommands", $"{AssemblyName}.encrypt;{AssemblyName}.decrypt");
                key.SetValue("Icon", ProgramPath);
            }

            RegisterContextMenuCommand("encrypt", "--en -d \"%1\"");
            RegisterContextMenuCommand("decrypt", "--de -d \"%1\"");

            Console.WriteLine("Context menu added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error adding context menu: " + ex.Message);
        }
    }

    private static void RegisterContextMenuCommand(string command, string arguments)
    {
        using RegistryKey baseReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using RegistryKey key = baseReg.CreateSubKey(CommandStorePath + $"{AssemblyName}.{command}");
        key.SetValue(null, Utility.FirstCharToUpper(command));
        using var commandKey = key.CreateSubKey("command");
        commandKey.SetValue(null, $"\"{ProgramPath}\" {arguments}");
    }

    private static void RemoveContextMenu()
    {
        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(DirectoryShellPath, false);
            Registry.LocalMachine.DeleteSubKeyTree(CommandStorePath + $"{AssemblyName}.encrypt", false);
            Registry.LocalMachine.DeleteSubKeyTree(CommandStorePath + $"{AssemblyName}.decrypt", false);

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
            using RegistryKey key = Registry.ClassesRoot.OpenSubKey(path);
            return key != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking registry key: {ex.Message}");
            return false;
        }
    }
}
