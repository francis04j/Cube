using Newtonsoft.Json;

namespace CubeLogic.TransactionsConverter.Configuration;

public class JsonDeserializerWrapper : IJsonDeserializer
{
    public T? Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}