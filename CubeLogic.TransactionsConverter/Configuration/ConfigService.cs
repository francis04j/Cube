using CubeLogic.TransactionsConverter.Entities;
using CubeLogic.TransactionsConverter.Errors;
using CubeLogic.TransactionsConverter.FileValidators;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CubeLogic.TransactionsConverter.Configuration;

public class ConfigService : IConfigService
{
    readonly IFileValidator _fileValidator;
    readonly IJsonDeserializer _jsonDeserializer;
    
    public ConfigService(IFileValidator fileValidator, IJsonDeserializer jsonDeserializer)
    {
        _fileValidator = fileValidator;
        _jsonDeserializer = jsonDeserializer;
    }
    
    public Result<Config> LoadConfig(string filePath)
    {
        var validationResult = _fileValidator.ValidateFile(filePath);
        if (validationResult.IsFailed)
            return validationResult;

        try
        {
            var config =  _jsonDeserializer.Deserialize<Config>(File.ReadAllText(filePath));
            if (config == null || string.IsNullOrWhiteSpace(config?.Timezone))
            {
                return Result.Fail(ErrorFactory.Create(ErrorCode.NullOrEmptyInput, $"No timezone specified, check config file"));
            }
            return Result.Ok(config);
        }
        catch (Exception ex)
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.UnexpectedError,$"Failed to load config file: {ex.Message}"));
        }
    }

}