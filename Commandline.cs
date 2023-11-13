using CommandLine;

namespace FileProtector;

[Verb("process", isDefault: true, HelpText = "Process a directory")]

internal class Options
{
    [Option('d', "directory", Required = true, HelpText = "Directory to be processed.")]
    public required string Directory { get; set; }

    [Option('p', "password", Required = true, HelpText = "Password to be used.")]
    public required string Password { get; set; }

    [Option("en", HelpText = "Will only encrypt.")]
    public bool EncryptOnly { get; set; }

    [Option("de", HelpText = "Will only decrypt.")]
    public bool DecryptOnly { get; set; }

    [Option('s', HelpText = "After decryption/encryption, will not delete previous older files.")]
    public bool SafeMode { get; set; }
}
[Verb("reset", HelpText = "Removes current password from system. Encrypted files will not be decrypted, be careful when using this.")]
internal class Reset
{
}

