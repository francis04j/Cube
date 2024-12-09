namespace CubeLogic.TransactionsConverter.Configuration;

public interface IJsonDeserializer
{
    T? Deserialize<T>(string json);
}