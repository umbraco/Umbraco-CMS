using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a tagged entity.
    /// </summary>
    /// <remarks>Note that it is the properties of an entity (like Content, Media, Members, etc.) that is tagged, 
    /// which is why this class is composed of a list of tagged properties and an Id reference to the actual entity.</remarks>
    public class TaggedEntity
    {
        public TaggedEntity(int entityId, IEnumerable<TaggedProperty> taggedProperties)
        {
            EntityId = entityId;
            TaggedProperties = taggedProperties;
        }

        /// <summary>
        /// Id of the entity, which is tagged
        /// </summary>
        public int EntityId { get; private set; }

        /// <summary>
        /// An enumerable list of tagged properties
        /// </summary>
        public IEnumerable<TaggedProperty> TaggedProperties { get; private set; }
    }
}