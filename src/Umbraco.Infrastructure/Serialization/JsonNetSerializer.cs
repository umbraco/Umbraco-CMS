using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class JsonNetSerializer : IJsonSerializer
{
    [Obsolete("This is a static field and shared between derived implementations. Use the Settings instance property instead. This field will be removed in v13.")]
    protected static readonly JsonSerializerSettings JsonSerializerSettings = new();

    protected JsonSerializerSettings Settings { get; } = new()
    {
        Converters = new List<JsonConverter> { new StringEnumConverter() },
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
    };

    public string Serialize(object? input) => JsonConvert.SerializeObject(input, Settings);

    public T? Deserialize<T>(string input) => JsonConvert.DeserializeObject<T>(input, Settings);

    public T? DeserializeSubset<T>(string input, string key)
    {
        ArgumentNullException.ThrowIfNull(key);

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
