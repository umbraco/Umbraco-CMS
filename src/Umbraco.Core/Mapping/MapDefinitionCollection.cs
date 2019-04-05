using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Mapping
{
    public class MapDefinitionCollection : BuilderCollectionBase<IMapDefinition>
    {
        public MapDefinitionCollection(IEnumerable<IMapDefinition> items)
            : base(items)
        { }
    }
}
