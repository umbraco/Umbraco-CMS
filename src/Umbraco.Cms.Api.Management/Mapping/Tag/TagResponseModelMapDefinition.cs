using Umbraco.Cms.Api.Management.ViewModels.Tag;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Tag;

public class TagResponseModelMapDefinition : IMapDefinition
{
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
