using System.Text;

namespace FileProtector;

public readonly struct Information(Dictionary<bool, string[]> allFiles, string password)
{
    public int EncryptCount { get; } = allFiles[false].Length;
    public int DecryptCount { get; } = allFiles[true].Length;
    public string Password { get; } = password;

    public override string ToString()
    {
        StringBuilder sb = new();
        _ = sb.AppendLine($"Using the password:'{Password}'")
          .AppendLine("You are about to process the following actions:")
          .AppendLine($"\t- Encrypting {EncryptCount} file(s)")
          .AppendLine($"\t- Decrypting {DecryptCount} file(s)");

        return sb.ToString();
    }
}
