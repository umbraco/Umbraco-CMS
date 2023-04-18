using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.PartialView;

public class PartialViewViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<PartialViewSnippet, PartialViewSnippetsViewModel>((_, _) => new PartialViewSnippetsViewModel{ Name = string.Empty, Content = string.Empty }, Map);
        mapper.Define<CreatePartialViewRequestModel, PartialViewCreateModel>((_, _) => new PartialViewCreateModel{Name = string.Empty}, Map);
    }

    private void Map(CreatePartialViewRequestModel source, PartialViewCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Content = source.Content;
        target.ParentPath = source.ParentPath;
    }

    private void Map(PartialViewSnippet source, PartialViewSnippetsViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Content = source.Content;
    }
}
