using CommandLine;

namespace FileProtector.CommandLineOptions;
[Verb("action", isDefault: true, HelpText = "Main action of this program, encrypting or decrypting.")]
internal class FileProcessingOptions
{
    [Option('d', "directory", Required = true, HelpText = "Specify the directory to be processed.")]
    public string Directory { get; set; } = "";

    [Option('p', "password", HelpText = "Set the password to be used.")]
    public string Password { get; set; } = "";

    [Option("en", HelpText = "Encrypt FilesToProcess only.")]
    public bool EncryptOnly { get; set; }

    [Option("de", HelpText = "Decrypt FilesToProcess only.")]
    public bool DecryptOnly { get; set; }

    [Option('s', HelpText = "After decryption/encryption, keep previous older FilesToProcess.")]
    public bool SafeMode { get; set; }

    [Option("name", HelpText = "Ignore encrypting file names.")]
    public bool IgnoreNames { get; set; }

    [Option('l', HelpText = "Limit the number of processed FilesToProcess. Use negative numbers to count from the end.")]
    public int Limit { get; set; }
}
