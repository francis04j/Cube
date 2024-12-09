namespace CubeLogic.TransactionsConverter.CustomCsvReader;


public interface ICsvReaderFactory
{
    ICsvReader CreateCsvReader(string inputPath);
}