using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Unity.Trimmer.Cli.Commands;
using Version = Unity.Trimmer.Cli.Commands.Version;

namespace unity.trimmer.cli
{
    internal static class Program
    {
        private static async Task<int> Main(params string[] args)
        {
            // Parse arguments
            RootCommand rootCommand = new RootCommand("Unity Trimmer CLI Tools.");
            
            // Trim
            var trimCommand = new Command("trim", "Trim an asset bundle (ex: 'unity_default_resources')");
            trimCommand.AddAlias("n");
            trimCommand.AddArgument(new Argument<string>("input"));
            trimCommand.AddArgument(new Argument<string>("output"));
            trimCommand.AddOption(new Option<string>(new []{"--classdata", "--c"}, () => "", "'classdata.tpk' file path"));
            trimCommand.AddOption(new Option<string>(new []{"--font", "--f"}, () => "", "Empty font file path"));
            trimCommand.Handler = CommandHandler.Create<string, string, string, string>(Trim.Execute);
            rootCommand.AddCommand(trimCommand);
            
            // Version
            var versionCommand = new Command("version", "Print version information");
            versionCommand.AddOption(new Option<bool>(new []{"--json", "--j"}, () => false, "JSON Output"));
            versionCommand.AddOption(new Option<bool>(new []{"--show-system", "--s"}, () => false, "Show system assemblies as well"));
            versionCommand.Handler = CommandHandler.Create<bool, bool>(Version.Execute);
            rootCommand.AddCommand(versionCommand);
            
            return await rootCommand.InvokeAsync(args);
        }
    }
}