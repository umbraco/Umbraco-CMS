using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    public class TaggedEntity
    {
        public TaggedEntity(int entityId, IEnumerable<TaggedProperty> taggedProperties)
        {
            EntityId = entityId;
            TaggedProperties = taggedProperties;
        }

        public int EntityId { get; private set; }
        public IEnumerable<TaggedProperty> TaggedProperties { get; private set; }
    }
}