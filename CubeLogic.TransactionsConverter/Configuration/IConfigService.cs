using CubeLogic.TransactionsConverter.Entities;
using FluentResults;

namespace CubeLogic.TransactionsConverter.Configuration;

public interface IConfigService
{
    Result<Config> LoadConfig(string filePath);
}