using System.Text;
using static FileProtector.Program;

namespace FileProtector;

public readonly struct Information
{
    public Information(Dictionary<bool, string[]> allFiles, Actions action)
    {
        EncryptCount = allFiles[false].Length;
        DecryptCount = allFiles[true].Length;
        Action = action;
        Console.WriteLine(ToString());
    }

    public int EncryptCount { get; }
    public int DecryptCount { get; }
    public Actions Action { get; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are about to process");

        switch (Action)
        {
            case Actions.Encrypt:
                sb.AppendLine($"{EncryptCount} files for encryption");
                break;
            case Actions.Decrypt:
                sb.AppendLine($"{DecryptCount} files for decryption");
                break;
            case Actions.Both:
                sb.AppendLine($"{EncryptCount} files for encryption");
                sb.AppendLine($"{DecryptCount} files for decryption");
                break;
        }

        return sb.ToString();
    }
}
