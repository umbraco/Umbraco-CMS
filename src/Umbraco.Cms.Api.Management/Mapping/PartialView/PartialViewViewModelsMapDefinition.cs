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
        mapper.Define<IPartialView, PartialViewResponseModel>((_, _ ) => new PartialViewResponseModel{Name = string.Empty, Path = string.Empty}, Map);
        mapper.Define<CreatePartialViewRequestModel, PartialViewCreateModel>((_, _) => new PartialViewCreateModel{Name = string.Empty}, Map);
        mapper.Define<UpdatePartialViewRequestModel, PartialViewUpdateModel>((_, _ ) => new PartialViewUpdateModel{ Content = string.Empty, ExistingPath = string.Empty, Name = string.Empty }, Map);
    }

    private void Map(UpdatePartialViewRequestModel source, PartialViewUpdateModel target, MapperContext context)
    {
        target.Name = source.Name;
        // TODO: Make content required in the request model
        target.Content = source.Content ?? string.Empty;
        target.ExistingPath = source.ExistingPath;
    }

    private void Map(IPartialView source, PartialViewResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Content = source.Content;
        target.Path = source.Path;
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
