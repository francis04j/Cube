namespace CubeLogic.TransactionsConverter.Errors;

public enum ErrorCode
{
    NullOrEmptyInput,
    UtCDateTimeConversionFailed,
    UnexpectedError,
    NoTimezoneSpecified,
    MissingField,
    InvalidDateTime,
    InvalidRecordType,
    InvalidJson,
    UnknownOrderType
}