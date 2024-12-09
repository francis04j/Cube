using CubeLogic.TransactionsConverter.Errors;
using FluentResults;
using TimeZoneConverter;

namespace CubeLogic.TransactionsConverter.TimezoneConverter;

public class DefaultUtCconverter : IUtCconverter
{
    public Result<string> Convert(string dateTime, string timezone)
    {
        if (string.IsNullOrWhiteSpace(dateTime))
        {
            return Result.Fail<string>(ErrorFactory.Create(ErrorCode.InvalidDateTime, $"Record time: ${dateTime} is invalid"));
        }
        
        if (string.IsNullOrWhiteSpace(timezone))
        {
            return Result.Fail<string>(ErrorFactory.Create(ErrorCode.NoTimezoneSpecified, $"Timezone: ${timezone} is invalid"));
        }
        
        try
        {
            var timeZoneInfo = TZConvert.GetTimeZoneInfo(timezone);
            var parsedDateTime = DateTime.Parse(dateTime);
            var localDateTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Unspecified);
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZoneInfo);
            var utcString = utcDateTime.ToString("O"); //ISO 8601 format. e.g. 2023-11-10T03:02:08.6671230Z
            return Result.Ok(utcString);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(ErrorFactory.Create(ErrorCode.UtCDateTimeConversionFailed, ex.Message));
        }
    }
}