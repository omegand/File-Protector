using FileProtector.CommandLineOptions;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace FileProtector;
internal static class PasswordStorage
{
    private const string MapName = "8b9c4777e1b15385f086b5c";

    public static void StartServer(string password)
    {
        try
        {
            string? currentExecutable = Environment.ProcessPath;
            if (currentExecutable is null)
            {
                Console.WriteLine("Could not find executable path, ignoring server launch.");
                return;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = currentExecutable,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = $"server -p {password}"
            };
            Process process = new()
            {
                StartInfo = startInfo
            };

            _ = process.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting server: {ex.Message}");
        }
    }

    public static int StorePassword(InternalServerOptions opts)
    {
        using MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(MapName, 1024);
        using MemoryMappedViewAccessor writer = mmf.CreateViewAccessor(0, 1024);
        byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(opts.Password);
        writer.WriteArray(0, passwordBytes, 0, passwordBytes.Length);
        while (true)
        {
            Thread.Sleep(10000);
        }

        return 1;
    }

    public static string GetPasswordFromServer()
    {
        using MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(MapName, 1024);
        using MemoryMappedViewAccessor reader = mmf.CreateViewAccessor(0, 1024);
        byte[] passwordBytes = new byte[1024];
        _ = reader.ReadArray(0, passwordBytes, 0, 1024);
        return System.Text.Encoding.UTF8.GetString(passwordBytes).Trim('\0');
    }
}