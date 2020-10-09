using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{

    /// <summary>
    /// Serializes/Deserializes <see cref="ContentCacheDataModel"/> document to the SQL Database as a string
    /// </summary>
    /// <remarks>
    /// Resolved from the <see cref="IContentCacheDataSerializerFactory"/>. This cannot be resolved from DI.
    /// </remarks>
    public interface IContentCacheDataSerializer
    {
        ContentCacheDataModel Deserialize(int contentTypeId, string stringData, byte[] byteData);
        ContentCacheDataSerializationResult Serialize(int contentTypeId, ContentCacheDataModel model);
    }

}
