using System.Diagnostics;
using System.IO.Pipes;

namespace FileProtector;
internal class ServerClientOperations
{
    private const string PipeName = "8b9c4777e1b15385f086b5c";
    private const int TimeOut = 100;

    public static void StartServer(string password)
    {
        try
        {
            string currentExecutable = Environment.ProcessPath;
            ProcessStartInfo startInfo = new()
            {
                FileName = currentExecutable,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = $"server -p {password} "
            };

            Process process = new()
            {
                StartInfo = startInfo
            };
            _ = process.Start();
        }
        catch (Exception)
        {
            Console.WriteLine("Password proccess already exists, ignoring new password until reboot.");
        }

    }

    public static void ServerLoop(Server data)
    {
        while (true)
        {
            using NamedPipeServerStream server = new(PipeName);
            server.WaitForConnection();

            using StreamWriter writer = new(server);
            writer.WriteLine(data.Password);
            writer.Flush();
        }
    }

    public static string GetPasswordFromServer()
    {
        try
        {
            string data = "";
            using (NamedPipeClientStream client = new(PipeName))
            {
                client.Connect(TimeOut);

                using StreamReader reader = new(client);
                data = reader.ReadLine();
            }
            return data;
        }
        catch (Exception)
        {
            return "";
        }
    }
}
