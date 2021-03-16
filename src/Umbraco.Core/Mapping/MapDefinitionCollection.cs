using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Mapping
{
    public class MapDefinitionCollection : BuilderCollectionBase<IMapDefinition>
    {
        public MapDefinitionCollection(IEnumerable<IMapDefinition> items)
            : base(items)
        { }
    }
}
