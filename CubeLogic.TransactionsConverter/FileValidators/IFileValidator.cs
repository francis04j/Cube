using FluentResults;

namespace CubeLogic.TransactionsConverter.FileValidators;

public interface IFileValidator
{
    Result ValidateFile(string filePath);
}