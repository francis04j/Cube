namespace CubeLogic.TransactionsConverter.CustomCsvReader;
using System.Globalization;
using CsvHelper.Configuration;

public class CsvReaderFactory : ICsvReaderFactory
{
    public ICsvReader CreateCsvReader(string inputPath)
    {
        var streamReader = new StreamReader(inputPath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture);
        return new CsvReaderWrapper(streamReader, config);
    }
    
}