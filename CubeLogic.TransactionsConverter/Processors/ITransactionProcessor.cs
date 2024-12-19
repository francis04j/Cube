using CubeLogic.TransactionsConverter.Entities;
using FluentResults;

namespace CubeLogic.TransactionsConverter.Processors;

public interface ITransactionProcessor
{
    Result ProcessTransactions(string inputPath, string outputPath, Config config);
}