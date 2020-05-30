using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Maps property aliases for nucache seralization and deserialization
    /// </summary>
    public interface IPublishedCachePropertyKeyMapper
    {
        /// <summary>
        /// Gets the key to use to serialize/deserialize the Property in nucache json
        /// </summary>
        /// <param name="property">Property being serialized/deserialized</param>
        /// <remarks>Use the shortest possible key that won't collide with any other key in order to keep the nucache json small</remarks>
        /// <returns>Key for property in nucache</returns>
        string ToCacheAlias(Property property);

        /// <summary>
        /// Gets the key to use to serialize/deserialize the Property in nucache json
        /// </summary>
        /// <param name="property">Property being serialized/deserialized</param>
        /// <remarks>Use the shortest possible key that won't collide with any other key in order to keep the nucache json small</remarks>
        /// <returns>Key for property in nucache</returns>
        string ToCacheAlias(IPublishedPropertyType property);

        /// <summary>
        /// Maps a nucache property key back to the umbraco property name
        /// </summary>
        /// <param name="cachePropertyKey">Key used as nucache key for the property</param>
        /// <returns>Umbraco Property Alias</returns>
        string ToPropertyAlias(string cachePropertyKey);

        /// <summary>
        /// Build the map
        /// </summary>
        /// <remarks>When properties modified for a document type the map will need rebuilding. Nucache may need to be rebuilt in the Database, file and memory</remarks>
        void BuildMap();
    }
}
