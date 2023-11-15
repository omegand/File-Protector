using System.Text;
using static FileProtector.Program;

namespace FileProtector;

public readonly struct Information
{
    public Information(Dictionary<bool, string[]> allFiles, Actions action, string password)
    {
        EncryptCount = allFiles[false].Length;
        DecryptCount = allFiles[true].Length;
        Action = action;
        Password = password;
    }

    public int EncryptCount { get; }
    public int DecryptCount { get; }
    public Actions Action { get; }
    public string Password { get; }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine($"Using the password: '{Password}'");
        sb.AppendLine("You are about to process the following actions:");

        switch (Action)
        {
            case Actions.Encrypt:
                sb.AppendLine($"\t- Encrypting {EncryptCount} file(s)");
                break;
            case Actions.Decrypt:
                sb.AppendLine($"\t- Decrypting {DecryptCount} file(s)");
                break;
            case Actions.Both:
                sb.AppendLine($"\t- Encrypting {EncryptCount} file(s)")
                  .AppendLine($"\t- Decrypting {DecryptCount} file(s)");
                break;
        }

        return sb.ToString();
    }

}
