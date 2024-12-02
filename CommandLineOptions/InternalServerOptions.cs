using CommandLine;

namespace FileProtector.CommandLineOptions;
[Verb("server", Hidden = true, HelpText = "For internal use. Do not call this.")]
internal class InternalServerOptions
{
    [Option('p', Hidden = true)]
    public string Password { get; set; } = "";
}