using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Mapping;

/// <summary>
/// Builds the <see cref="MapDefinitionCollection"/> by allowing registration of <see cref="IMapDefinition"/> instances.
/// </summary>
/// <remarks>
/// Use this builder to register custom map definitions that define how objects are mapped from one type to another.
/// </remarks>
public class MapDefinitionCollectionBuilder : SetCollectionBuilderBase<MapDefinitionCollectionBuilder, MapDefinitionCollection, IMapDefinition>
{
    /// <inheritdoc />
    protected override MapDefinitionCollectionBuilder This => this;

    /// <inheritdoc />
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient;
}
