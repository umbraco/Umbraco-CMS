using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class JsonNetSerializer : IJsonSerializer
{
    protected static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Converters = new List<JsonConverter> { new StringEnumConverter() },
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
    };

    public string Serialize(object? input) => JsonConvert.SerializeObject(input, JsonSerializerSettings);

    public T? Deserialize<T>(string input) => JsonConvert.DeserializeObject<T>(input, JsonSerializerSettings);

    public T? DeserializeSubset<T>(string input, string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        JObject? root = Deserialize<JObject>(input);
        JToken? jToken = root?.SelectToken(key);

        return jToken switch
        {
            JArray jArray => jArray.ToObject<T>(),
            JObject jObject => jObject.ToObject<T>(),
            _ => jToken is null ? default : jToken.Value<T>(),
        };
    }
}
