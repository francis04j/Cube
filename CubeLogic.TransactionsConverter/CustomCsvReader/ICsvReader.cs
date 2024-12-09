namespace CubeLogic.TransactionsConverter.CustomCsvReader;

public interface ICsvReader : IDisposable
{
    bool Read();
    bool ReadHeader();
    IEnumerable<T> GetRecords<T>();
}