using FluentResults;

namespace CubeLogic.TransactionsConverter.Errors;

public static class ErrorFactory
{
    public static Error Create(ErrorCode code, string message)
    {
        return new Error(message).WithMetadata("Code", code.ToString());
    }
}