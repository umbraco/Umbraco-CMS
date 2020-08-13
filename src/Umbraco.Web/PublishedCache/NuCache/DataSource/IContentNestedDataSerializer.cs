namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // TODO: We need better names if possible, not sure why the class is called ContentNested in the first place

    /// <summary>
    /// Serializes/Deserializes <see cref="ContentNestedData"/> document to the SQL Database as bytes
    /// </summary>
    public interface IContentNestedDataByteSerializer
    {
        ContentNestedData DeserializeBytes(byte[] data);
        byte[] SerializeBytes(ContentNestedData nestedData);
    }

    /// <summary>
    /// Serializes/Deserializes <see cref="ContentNestedData"/> document to the SQL Database as a string
    /// </summary>
    public interface IContentNestedDataSerializer
    {
        ContentNestedData Deserialize(string data);        
        string Serialize(ContentNestedData nestedData);        
    }
}
