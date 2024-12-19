using CommandLine;

namespace CubeLogic.TransactionsConverter;

public class Options
{
    [Option('c', "config", Required = false, HelpText = "Path to the config file.", Default = "config.json")]
    public string ConfigPath { get; set; }

    [Option('i', "input", Required = false, HelpText = "Path to the input transactions file.", Default = "inputTransactions.csv")]
    public string InputPath { get; set; }

    [Option('o', "output", Required = false, HelpText = "Path to the output file.", Default = "/app/output/output.csv")]
    public string OutputPath { get; set; }
}