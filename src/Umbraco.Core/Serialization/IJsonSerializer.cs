namespace Umbraco.Cms.Core.Serialization;

public interface IJsonSerializer
{
    string Serialize(object? input);

    T? Deserialize<T>(string input);

    [Obsolete("This will be removed in v13")]
    T? DeserializeSubset<T>(string input, string key);
}
