namespace CubeLogic.TransactionsConverter.CsvWriter;

public interface ICsvWriter : IDisposable
{
    void WriteHeader<T>();
    void NextRecord();
    void WriteRecord<T>(T record);
}