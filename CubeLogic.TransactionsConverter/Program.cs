using CubeLogic.TransactionsConverter;
using CubeLogic.TransactionsConverter.Configuration;
using CubeLogic.TransactionsConverter.CsvWriter;
using CubeLogic.TransactionsConverter.CustomCsvReader;
using CubeLogic.TransactionsConverter.FileValidators;
using CubeLogic.TransactionsConverter.Processors;
using CubeLogic.TransactionsConverter.TimezoneConverter;
using Microsoft.Extensions.Logging;

// PLEASE NOTE: the input and config files are always copied into bin directory. This sis a build action
//CAN UPDATE TO WRITE/READ in Windows special Data folders
//https://stackoverflow.com/questions/57638466/how-to-create-text-file-in-net-project-folder-not-in-bin-debug-folder-where-is

/*
 *var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                 "log1.txt");
   using (StreamWriter file = File.AppendText(fileName))
   {
       file.WriteLine("Hello from the text file");
   }
 */

namespace CubeLogic.TransactionsConverter;
using Microsoft.Extensions.Logging;

public class Program
{
    
    static void Main(string[] args)
    {
        string configPath = "config.json";
        string inputPath = "inputTransactions.csv";
        string outputPath = "output.csv";

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<TransactionProcessor>(); 
        var configService = new ConfigService(new JsonFileValidator(), new JsonDeserializerWrapper());
       
        var configResult = configService.LoadConfig(configPath);
        if (configResult.IsFailed)
        {
            logger.LogError(configResult.Errors[0].Message);
            return;
        }

        var config = configResult.Value;

        // Process CSV using streaming
        var transactionProcessor = new TransactionProcessor(new DefaultUtCconverter(), new CsvReaderFactory(), new CsvWriterFactory(), logger);
        var processResult = transactionProcessor.ProcessTransactions(inputPath, outputPath, config);
        if (processResult.IsFailed)
        {
            logger.LogError(processResult.Errors[0].Message);
            return;
        }

        logger.LogInformation("Processing complete. Output saved to " + outputPath);
    }
}