using System.Collections.Concurrent;
using System.Globalization;
using FluentResults;
using CsvHelper;
using CsvHelper.Configuration;
using CubeLogic.TransactionsConverter;
using CubeLogic.TransactionsConverter.CsvWriter;
using CubeLogic.TransactionsConverter.CustomCsvReader;
using CubeLogic.TransactionsConverter.Entities;
using CubeLogic.TransactionsConverter.Errors;
using CubeLogic.TransactionsConverter.TimezoneConverter;
using Microsoft.Extensions.Logging;
using MissingFieldException = CsvHelper.MissingFieldException; 

namespace CubeLogic.TransactionsConverter.Processors;

public class TransactionProcessor
{
    readonly IUtCconverter _utCconverter;
    readonly ICsvReaderFactory _csvReaderFactory;
    readonly ICsvWriterFactory _csvWriterFactory;
    readonly ILogger<TransactionProcessor> _logger;
    
    public TransactionProcessor(IUtCconverter utCconverter, 
        ICsvReaderFactory csvReaderFactory, 
        ICsvWriterFactory csvWriterFactory, ILogger<TransactionProcessor> logger)
    {
        _utCconverter = utCconverter;
        _csvReaderFactory = csvReaderFactory;
        _csvWriterFactory = csvWriterFactory;
        _logger = logger;
    }
    
    public Result ProcessTransactions(string inputPath, string outputPath, Config config)
    {
        _logger.LogInformation("Processing transactions from {inputPath}", inputPath);
        var result = InputParametersValidator.ValidateInput(inputPath, outputPath, config);
        if(result.IsFailed){
            return result;
        }

        var instrumentsDict = config.Instruments.ToDictionary(i => i.InstrumentId);
        
        var revisionCounters = new Dictionary<string, int>();
        var lastProcessedPrice = new Dictionary<string, decimal?>();

        try
        {
            using var csvReader = _csvReaderFactory.CreateCsvReader(inputPath);
            csvReader.Read();
            csvReader.ReadHeader();

            using var csvWriter = _csvWriterFactory.CreateCsvWriter(outputPath);
            csvWriter.WriteHeader<OutputTransaction>();
            csvWriter.NextRecord();

            foreach (var record in csvReader.GetRecords<InputTransaction>())
            {
                var utcDateTimeResult = _utCconverter.Convert(record.DateTime, config.Timezone);
                if (utcDateTimeResult.IsFailed)
                {
                    Console.WriteLine(utcDateTimeResult.Errors[0].Message);
                    return Result.Fail(ErrorFactory.Create(ErrorCode.InvalidDateTime, utcDateTimeResult.Errors[0].Message));
                }

                var utcDateTime = utcDateTimeResult.Value;

                instrumentsDict.TryGetValue(record.InstrumentId, out var instrument);
                string country = instrument?.Country ?? "Error";
                string instrumentName = instrument?.InstrumentName ?? "Error";

                if (instrument == null)
                {
                    _logger.LogWarning($"InstrumentId {record.InstrumentId} not found in config.json.");
                }

                var typeResult = MapType(record.Type);
                if (typeResult.IsFailed)
                {
                    _logger.LogError(typeResult.Errors[0].Message);
                    return Result.Fail(ErrorFactory.Create(ErrorCode.InvalidRecordType, typeResult.Errors[0].Message));
                }

                var type = typeResult.Value;

                if (ShouldSkipRecordingOrderRevision(type, record, lastProcessedPrice, revisionCounters))
                {
                    continue;
                }else if (type == "UPDATE")
                {
                    lastProcessedPrice[record.OrderId] = record.Price;
                    revisionCounters[record.OrderId]++;
                        
                }
                else
                {
                    var revision = revisionCounters.GetValueOrDefault(record.OrderId);
                    if (revision == 0)
                    {
                        revisionCounters[record.OrderId] = 1;
                    }
                }

                var outputTransaction = new OutputTransaction
                (
                    record.OrderId,
                    type,
                    revisionCounters[record.OrderId],
                    utcDateTime,
                    record.Price,
                    country,
                    instrumentName
                );

                csvWriter.WriteRecord(outputTransaction);
                csvWriter.NextRecord();
            }
            _logger.LogInformation("Finished Processing transactions from {inputPath}", inputPath);
            return Result.Ok();
        }
        catch (MissingFieldException ex)
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.MissingField, ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.UnexpectedError, ex.Message));
        }
    }

    private static bool ShouldSkipRecordingOrderRevision(string type, InputTransaction record, Dictionary<string, decimal?> lastProcessedPrice,
        Dictionary<string, int> revisionCounters)
    {
        if (type == "UPDATE")
        {
            var key = record.OrderId;
            var lastPrice = lastProcessedPrice.GetValueOrDefault(key);
            if (lastPrice == record.Price)
            {
                return true;
            }

        }


        return false;
    }


    private static Result<string> MapType(string type)
    {
            return type switch
            {
                "AddOrder" => Result.Ok("ADD"),
                "UpdateOrder" => Result.Ok("UPDATE"),
                "DeleteOrder" => Result.Ok("DELETE"),
                _ => Result.Fail<string>(ErrorFactory.Create(ErrorCode.UnknownOrderType, $"Unknown Type value: {type}"))
            };
    }
}