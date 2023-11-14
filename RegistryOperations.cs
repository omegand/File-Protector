using Microsoft.Win32;

namespace FileProtector;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "CONSTRUCTOR CHECKS FOR WINDOWS")]
public class RegistryOperations
{
    public static readonly string gibberishRegistryPath = @"SOFTWARE\Yahoo\Common\Okz";
    public static readonly string registryAesKey = "yta";
    static RegistryOperations()
    {
        WindowsCheck();
    }
    public static void SaveIntoRegistry(byte[] value)
    {

        using RegistryKey key = Registry.CurrentUser.CreateSubKey(gibberishRegistryPath);
        key.SetValue(registryAesKey, value, RegistryValueKind.Binary);
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
    /// Retrieves a registry object or returns null if not found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static object? GetValue(string key)
    {
        var registryKey = Registry.CurrentUser.OpenSubKey(gibberishRegistryPath, false);

        return registryKey?.GetValue(key);
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
