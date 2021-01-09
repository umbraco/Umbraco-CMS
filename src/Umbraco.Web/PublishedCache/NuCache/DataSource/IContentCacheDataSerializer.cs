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
        /// <summary>
        /// Deserialize the data into a <see cref="ContentCacheDataModel"/>
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="stringData"></param>
        /// <param name="byteData"></param>
        /// <returns></returns>
        ContentCacheDataModel Deserialize(int contentTypeId, string stringData, byte[] byteData);

        /// <summary>
        /// Serializes the <see cref="ContentCacheDataModel"/> 
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        ContentCacheDataSerializationResult Serialize(int contentTypeId, ContentCacheDataModel model);
    }

}
