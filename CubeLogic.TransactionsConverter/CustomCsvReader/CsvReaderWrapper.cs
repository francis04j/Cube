using CsvHelper;
using CsvHelper.Configuration;

namespace CubeLogic.TransactionsConverter.CustomCsvReader;

public class CsvReaderWrapper : ICsvReader
{
    private readonly CsvReader _csvReader;
    private readonly StreamReader _streamReader;

    public CsvReaderWrapper(StreamReader reader, CsvConfiguration config)
    {
        _streamReader = reader;
        _csvReader = new CsvReader(reader, config);
    }

    public bool Read() => _csvReader.Read();

    public bool ReadHeader() => _csvReader.ReadHeader();

    public IEnumerable<T> GetRecords<T>() => _csvReader.GetRecords<T>();

    public void Dispose()
    {
        _csvReader.Dispose();
        _streamReader.Dispose();
    }
}