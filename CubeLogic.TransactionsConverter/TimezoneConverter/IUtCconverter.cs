using FluentResults;

namespace CubeLogic.TransactionsConverter.TimezoneConverter;

public interface IUtCconverter
{
    Result<string> Convert(string dateTime, string timezone);
}