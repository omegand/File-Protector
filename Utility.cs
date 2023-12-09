using System.Security.Principal;
using System.Text;

namespace FileProtector;

public class Utility
{
    public static void PrintArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Console.WriteLine(array[i].ToString());
        }
    }

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

    public static bool BytesEqual(byte[] a, byte[] b)
    {
        int xor = a.Length ^ b.Length;

        for (int i = 0; i < a.Length && i < b.Length; ++i)
        {
            xor |= a[i] ^ b[i];
        }
        return xor == 0;
    }

    public static string FirstCharToUpper(string input)
    {
        if (input is null or "")
        {
            Console.WriteLine("Cannot capitalize string as it's null or empty.");
            return input;
        }
        return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
    }
    public static void VerifyWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("There's currently only Windows support for this action.");
            ExitWithInput(5);
        }
    }

    public static void VerifyAdmin()
    {
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
