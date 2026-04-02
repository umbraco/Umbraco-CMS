using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Mapping;

/// <summary>
/// Represents a collection of <see cref="IMapDefinition"/> instances used for object mapping.
/// </summary>
public class MapDefinitionCollection : BuilderCollectionBase<IMapDefinition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapDefinitionCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that provides the map definitions.</param>
    public MapDefinitionCollection(Func<IEnumerable<IMapDefinition>> items)
        : base(items)
    {
    }
}
