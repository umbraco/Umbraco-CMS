using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Mapping
{
    public class MapDefinitionCollectionBuilder : SetCollectionBuilderBase<MapDefinitionCollectionBuilder, MapDefinitionCollection, IMapDefinition>
    {
        protected override MapDefinitionCollectionBuilder This => this;

        protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient;
    }
}
