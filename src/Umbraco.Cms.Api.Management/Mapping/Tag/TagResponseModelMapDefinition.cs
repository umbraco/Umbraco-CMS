using Umbraco.Cms.Api.Management.ViewModels.Tag;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Tag;

/// <summary>
/// Provides mapping configuration between tag entities and <see cref="TagResponseModel"/> instances.
/// </summary>
public class TagResponseModelMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures the mapping between <see cref="ITag"/> and <see cref="TagResponseModel"/>.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mapping.</param>
    public void DefineMaps(IUmbracoMapper mapper) =>
        mapper.Define<ITag, TagResponseModel>((_, _) => new TagResponseModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(ITag source, TagResponseModel target, MapperContext context)
    {
        target.Group = source.Group;
        target.Id = source.Key;
        target.NodeCount = source.NodeCount;
        target.Text = source.Text;
    }
}
