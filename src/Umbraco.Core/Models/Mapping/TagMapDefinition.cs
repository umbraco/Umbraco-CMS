using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Defines the mapping configuration for tag entities to tag models.
/// </summary>
public class TagMapDefinition : IMapDefinition
{
    /// <summary>
    ///     Defines the mapping from <see cref="ITag" /> to <see cref="TagModel" />.
    /// </summary>
    /// <param name="mapper">The mapper to configure.</param>
    public void DefineMaps(IUmbracoMapper mapper) =>
        mapper.Define<ITag, TagModel>((source, context) => new TagModel(), Map);

    // Umbraco.Code.MapAll
    /// <summary>
    ///     Maps an <see cref="ITag" /> source to a <see cref="TagModel" /> target.
    /// </summary>
    /// <param name="source">The source tag entity.</param>
    /// <param name="target">The target tag model.</param>
    /// <param name="context">The mapper context.</param>
    private static void Map(ITag source, TagModel target, MapperContext context)
    {
        target.Id = source.Id;
        target.Text = source.Text;
        target.Group = source.Group;
        target.NodeCount = source.NodeCount;
    }
}
