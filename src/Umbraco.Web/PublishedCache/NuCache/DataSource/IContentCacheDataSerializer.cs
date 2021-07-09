﻿using Umbraco.Core.Models;

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
        ContentCacheDataModel Deserialize(IReadOnlyContentBase content, string stringData, byte[] byteData);

        /// <summary>
        /// Serializes the <see cref="ContentCacheDataModel"/> 
        /// </summary>
        ContentCacheDataSerializationResult Serialize(IReadOnlyContentBase content, ContentCacheDataModel model);
    }

}
