using CommandLine;

namespace FileProtector;
[Verb("action", isDefault: true, HelpText = "Main action of this program, encrypting or decrypting.")]
internal class Options
{
    [Option('d', "directory", Required = true, HelpText = "Specify the directory to be processed.")]
    public required string Directory { get; set; }

    [Option('p', "password", HelpText = "Set the password to be used.")]
    public required string Password { get; set; }

    [Option("en", HelpText = "Encrypt files only.")]
    public bool EncryptOnly { get; set; }

    [Option("de", HelpText = "Decrypt files only.")]
    public bool DecryptOnly { get; set; }

    [Option('s', HelpText = "After decryption/encryption, keep previous older files.")]
    public bool SafeMode { get; set; }

    [Option('l', HelpText = "Limit the number of processed files. Use negative numbers to count from the end.")]
    public int Limit { get; set; }
}

[Verb("togglecontext", HelpText = "Add/Remove this program to the Windows right-click context menu.")]
internal class ToggleContextMenu
{
}