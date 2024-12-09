namespace CubeLogic.TransactionsConverter.CsvWriter;

public interface ICsvWriterFactory
{
    ICsvWriter CreateCsvWriter(string filePath);
}