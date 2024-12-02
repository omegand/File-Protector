using System.Security.Principal;
using System.Text;

namespace FileProtector;

public static class Utility
{
    public static string GetInput(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("\nProblem with input, try again.");
            return GetInput(prompt);
        }

        return input;
    }

    public static bool ConfirmAction(string message)
    {
        Console.WriteLine(message);
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

    public static string FirstCharToUpper(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Failed to capitalize string as input is null, empty or a whitespace.");
            return input;
        }
        return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
    }

    public static bool IsWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("There's currently only Windows support for this action.");
            ExitWithInput(5);
            return false;
        }

        return true;
    }

    public static void VerifyIsWindowsAdmin()
    {
        if (!IsWindows())
        {
            return;
        }

        WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
        WindowsPrincipal windowsPrincipal = new(windowsIdentity);

        if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            Console.WriteLine("You need to run the program as administrator to do this.");
            ExitWithInput(4);
        }
    }

    public static void ExitWithInput(int code)
    {
        Console.WriteLine("Press any key to exit...");
        _ = Console.ReadKey();
        Environment.Exit(code);
    }

    public static byte[] ToBytes(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }
}
