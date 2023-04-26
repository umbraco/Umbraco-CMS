using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Snippets;

namespace Umbraco.Cms.Api.Management.Mapping.PartialView;

public class PartialViewViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<PartialViewSnippet, PartialViewSnippetResponseModel>((_, _) => new PartialViewSnippetResponseModel { Name = string.Empty, Content = string.Empty }, Map);
        mapper.Define<IPartialView, PartialViewResponseModel>((_, _) => new PartialViewResponseModel { Name = string.Empty, Path = string.Empty, Content = string.Empty }, Map);
        mapper.Define<CreatePartialViewRequestModel, PartialViewCreateModel>((_, _) => new PartialViewCreateModel { Name = string.Empty }, Map);
        mapper.Define<UpdatePartialViewRequestModel, PartialViewUpdateModel>((_, _) => new PartialViewUpdateModel { Content = string.Empty, ExistingPath = string.Empty, Name = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(UpdatePartialViewRequestModel source, PartialViewUpdateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Content = source.Content;
        target.ExistingPath = source.ExistingPath;
    }

    // Umbraco.Code.MapAll
    private void Map(IPartialView source, PartialViewResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Content = source.Content ?? string.Empty;
        target.Path = source.Path;
    }

    // Umbraco.Code.MapAll
    private void Map(CreatePartialViewRequestModel source, PartialViewCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Content = source.Content;
        target.ParentPath = source.ParentPath;
    }

    // Umbraco.Code.MapAll
    private void Map(PartialViewSnippet source, PartialViewSnippetResponseModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Content = source.Content;
    }
}
