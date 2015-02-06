using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a tagged property on an entity.
    /// </summary>
    public class TaggedProperty
    {
        public TaggedProperty(int propertyTypeId, string propertyTypeAlias, IEnumerable<ITag> tags)
        {
            PropertyTypeId = propertyTypeId;
            PropertyTypeAlias = propertyTypeAlias;
            Tags = tags;
        }

        /// <summary>
        /// Id of the PropertyType, which this tagged property is based on
        /// </summary>
        public int PropertyTypeId { get; private set; }

        /// <summary>
        /// Alias of the PropertyType, which this tagged property is based on
        /// </summary>
        public string PropertyTypeAlias { get; private set; }

        /// <summary>
        /// An enumerable list of Tags for the property
        /// </summary>
        public IEnumerable<ITag> Tags { get; private set; }
    }
}