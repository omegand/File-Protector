using System.Text;

namespace FileProtector;

public readonly struct Information
{
    public Information(Dictionary<bool, string[]> allFiles, string password)
    {
        EncryptCount = allFiles[false].Length;
        DecryptCount = allFiles[true].Length;
        Password = password;
    }

    public int EncryptCount { get; }
    public int DecryptCount { get; }
    public string Password { get; }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine($"Using the password: '{Password}'")
          .AppendLine("You are about to process the following actions:")
          .AppendLine($"\t- Encrypting {EncryptCount} file(s)")
          .AppendLine($"\t- Decrypting {DecryptCount} file(s)");

        return sb.ToString();
    }
}
