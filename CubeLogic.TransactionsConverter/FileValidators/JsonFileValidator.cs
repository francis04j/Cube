using CubeLogic.TransactionsConverter.Errors;
using FluentResults;
using Newtonsoft.Json;

namespace CubeLogic.TransactionsConverter.FileValidators;

public class JsonFileValidator : IFileValidator
{
    public Result ValidateFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.NullOrEmptyInput, $"Given {filePath} is null or empty."));
        }
        
        try
        {
            JsonTextReader reader = new JsonTextReader(new StreamReader(filePath));
            while (reader.Read()) { }
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.InvalidJson,$"Invalid JSON format in file: {filePath}. Details: {ex.Message}"));
        }
    }
}