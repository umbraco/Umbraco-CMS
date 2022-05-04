namespace Umbraco.Cms.Core.Serialization
{
    public interface IJsonSerializer
    {
        string Serialize(object? input);

        T? Deserialize<T>(string input);

        T? DeserializeSubset<T>(string input, string key);
    }
}
