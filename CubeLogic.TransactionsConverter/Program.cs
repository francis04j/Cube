using CubeLogic.TransactionsConverter;
using CubeLogic.TransactionsConverter.Configuration;
using CubeLogic.TransactionsConverter.CsvWriter;
using CubeLogic.TransactionsConverter.CustomCsvReader;
using CubeLogic.TransactionsConverter.FileValidators;
using CubeLogic.TransactionsConverter.Processors;
using CubeLogic.TransactionsConverter.TimezoneConverter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CubeLogic.TransactionsConverter;
using Microsoft.Extensions.Logging;
using CommandLine;

public class Program
{
    static void Main(string[] args)
    {
        var host = AppHostStartup.CreateAppHost();
        var logger = ActivatorUtilities.GetServiceOrCreateInstance<ILogger<Program>>(host.Services);

        // Parse arguments and handle options
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                logger.LogInformation($"Config Path: {options.ConfigPath}");
                logger.LogInformation($"Input Path: {options.InputPath}");
                logger.LogInformation($"Output Path: {options.OutputPath}");
                
                var configService = ActivatorUtilities.GetServiceOrCreateInstance<IConfigService>(host.Services);
                var configResult = configService.LoadConfig(options.ConfigPath);
                if (configResult.IsFailed)
                {
                    logger.LogError(configResult.Errors[0].Message);
                    return;
                }

                var config = configResult.Value;

                // Process CSV using streaming
                var transactionProcessor = ActivatorUtilities.GetServiceOrCreateInstance<ITransactionProcessor>(host.Services);
                var processResult = transactionProcessor.ProcessTransactions(options.InputPath, options.OutputPath, config);
                if (processResult.IsFailed)
                {
                    logger.LogError(processResult.Errors[0].Message);
                    return;
                }

                logger.LogInformation("Processing complete. Output saved to {outputPath}", options.OutputPath);
            })
            .WithNotParsed(errors =>
            {
                logger.LogInformation("Failed to parse arguments. Use --help for usage instructions.");
            });

        
    }
}