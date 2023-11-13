using Microsoft.Win32;

namespace FileProtector;

public class RegistryOperations
{
    public static readonly string gibberishRegistryPath = @"SOFTWARE\Yahoo\Common\Okz";
    public static readonly string registryKey = "yta";
    static RegistryOperations()
    {
        WindowsCheck();
    }
    public static void SaveIntoRegistry(string value)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(gibberishRegistryPath);
        key.SetValue(registryKey, value);
    }

    public static void DeletePasswordFromRegistry()
    {
        Console.WriteLine("Attempting to delete the password...");
        try
        {
            Registry.CurrentUser.DeleteSubKey(gibberishRegistryPath);
            Console.WriteLine("Password deleted successfully.");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot remove password, most likely because it does not exist.");
            Console.WriteLine(ex);
            Environment.Exit(1);
        }
    }
    /// <summary>
    /// Gets password object, or null if not found.
    /// </summary>
    /// <returns></returns>
    public static object? GetPassword()
    {
        return Registry.CurrentUser.OpenSubKey(gibberishRegistryPath)?.GetValue(registryKey);
    }

    private static void WindowsCheck()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("There's currently only Windows support.");
            Environment.Exit(1);
        }
    }
}
