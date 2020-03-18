namespace Umbraco.Core.Serialization
{
    public interface IJsonSerializer
    {
        string Serialize(object input);

        T Deserialize<T>(string input);
    }
}
