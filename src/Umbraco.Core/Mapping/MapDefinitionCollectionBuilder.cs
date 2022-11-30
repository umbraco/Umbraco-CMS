using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Mapping;

public class MapDefinitionCollectionBuilder : SetCollectionBuilderBase<MapDefinitionCollectionBuilder, MapDefinitionCollection, IMapDefinition>
{
    protected override MapDefinitionCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient;
}
