using System.Globalization;
using CsvHelper.Configuration;

namespace CubeLogic.TransactionsConverter.CsvWriter;

public class CsvWriterFactory : ICsvWriterFactory
{
    public ICsvWriter CreateCsvWriter(string filePath)
    {
        var writer = new StreamWriter(filePath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture);
        return new CsvWriterWrapper(writer, config);
    }
}