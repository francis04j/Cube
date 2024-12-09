using CsvHelper.Configuration;

namespace CubeLogic.TransactionsConverter.CsvWriter;

public class CsvWriterWrapper : ICsvWriter
{
    private readonly CsvHelper.CsvWriter _csvWriter;
    private readonly StreamWriter _streamWriter;
    private readonly CsvConfiguration _config;
    
    public CsvWriterWrapper(StreamWriter streamWriter, CsvConfiguration config)
    {
        _streamWriter = streamWriter;
        _config = config;
        _csvWriter = new CsvHelper.CsvWriter(_streamWriter, _config);
    }

    public void WriteHeader<T>() => _csvWriter.WriteHeader<T>();

    public void NextRecord() => _csvWriter.NextRecord();

    public void WriteRecord<T>(T record) => _csvWriter.WriteRecord(record);

    public void Dispose()
    {
        _csvWriter.Dispose();
        _streamWriter.Dispose();
    }
}