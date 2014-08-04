using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    public class TaggedProperty
    {
        public TaggedProperty(int propertyTypeId, string propertyTypeAlias, IEnumerable<ITag> tags)
        {
            PropertyTypeId = propertyTypeId;
            PropertyTypeAlias = propertyTypeAlias;
            Tags = tags;
        }

        public int PropertyTypeId { get; private set; }
        public string PropertyTypeAlias { get; private set; }
        public IEnumerable<ITag> Tags { get; private set; }
    }
}