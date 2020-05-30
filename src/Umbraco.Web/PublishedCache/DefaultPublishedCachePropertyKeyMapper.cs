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
    /// Performs no mapping. Uses original property alias
    /// </summary>
    public class DefaultPublishedCachePropertyKeyMapper : IPublishedCachePropertyKeyMapper
    {
        /// <inheritdoc/>
        public void BuildMap()
        {
            //Not required
        }

        /// <inheritdoc/>
        public string ToCacheAlias(Property property)
        {
            return property.Alias;
        }
        /// <inheritdoc/>
        public string ToCacheAlias(IPublishedPropertyType property)
        {
            return property.Alias;
        }

        /// <inheritdoc/>
        public string ToPropertyAlias(string cachePropertyKey)
        {
            return cachePropertyKey;
        }
    }
}
