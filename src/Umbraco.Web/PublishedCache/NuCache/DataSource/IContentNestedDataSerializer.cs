namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // TODO: We need better names if possible, not sure why the class is called ContentNested in the first place

    /// <summary>
    /// Serializes/Deserializes <see cref="ContentNestedData"/> document to the SQL Database as bytes
    /// </summary>
    public interface IContentNestedDataByteSerializer : IContentNestedDataSerializer
    {
        ContentNestedData DeserializeBytes(int contentTypeId, byte[] data);
        byte[] SerializeBytes(int contentTypeId, ContentNestedData nestedData);
    }

    /// <summary>
    /// Serializes/Deserializes <see cref="ContentNestedData"/> document to the SQL Database as a string
    /// </summary>
    public interface IContentNestedDataSerializer
    {
        ContentNestedData Deserialize(int contentTypeId, string data);
        string Serialize(int contentTypeId, ContentNestedData nestedData);        
    }
}
