using CubeLogic.TransactionsConverter.Configuration;
using CubeLogic.TransactionsConverter.CsvWriter;
using CubeLogic.TransactionsConverter.CustomCsvReader;
using CubeLogic.TransactionsConverter.FileValidators;
using CubeLogic.TransactionsConverter.Processors;
using CubeLogic.TransactionsConverter.TimezoneConverter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CubeLogic.TransactionsConverter;

public class AppHostStartup
{
    public static IHost CreateAppHost()
    {
        var builder = new ConfigurationBuilder();
        
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables() //TODO: check if json file override env variables
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        // Specifying the configuration for serilog
        Log.Logger = new LoggerConfiguration() // initiate the logger configuration
            .ReadFrom.Configuration(builder.Build()) // connect serilog to our configuration folder
            .CreateLogger(); //initialise the logger
        
        Log.Logger.Information("Application Starting");

        var host = Host.CreateDefaultBuilder() // Initialising the Host 
            .ConfigureServices((context, services) => { // Adding the DI container for configuration
                services.AddTransient<IJsonDeserializer, JsonDeserializerWrapper>();    
                services.AddTransient<IFileValidator, JsonFileValidator>();
                services.AddTransient<IConfigService, ConfigService>();
                services.AddTransient<ITransactionProcessor, TransactionProcessor>(); // Add transiant mean give me an instance each it is being requested.**
                services.AddTransient<IUtCconverter, DefaultUtCconverter>();
                services.AddTransient<ICsvReaderFactory, CsvReaderFactory>();
                services.AddTransient<ICsvWriterFactory, CsvWriterFactory>();
            })
            .UseSerilog() 
            .Build();

        return host;

    }
}