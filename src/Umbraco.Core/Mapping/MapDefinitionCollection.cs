using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Mapping;

public class MapDefinitionCollection : BuilderCollectionBase<IMapDefinition>
{
    public MapDefinitionCollection(Func<IEnumerable<IMapDefinition>> items)
        : base(items)
    {
    }
}
