using CubeLogic.TransactionsConverter.Entities;
using CubeLogic.TransactionsConverter.Errors;
using FluentResults;

namespace CubeLogic.TransactionsConverter.Processors;

public class InputParametersValidator
{
    //TODO: use FluentValidator
    public static Result ValidateInput(string inputPath, string outputPath, Config config)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.NullOrEmptyInput, $"Given inputPath: {inputPath}, cannot be null or empty"));
        }
        
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.NullOrEmptyInput, $"Given outputPath: {outputPath}, cannot be null or empty"));
        }
        
        if (string.IsNullOrWhiteSpace(config.Timezone))
        {
            return Result.Fail(ErrorFactory.Create(ErrorCode.NoTimezoneSpecified,"No timezone specified, check if config.json file is correct"));
        }

        return Result.Ok();
    }
}