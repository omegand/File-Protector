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

        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("\nProblem with input, try again.");
            return GetInput(prompt);
        }

        return input;
    }
}
