using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Stylesheet;

public class StylesheetviewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IStylesheet, StylesheetResponseModel>((_, _) => new StylesheetResponseModel { Content = string.Empty, Name = string.Empty, Path = string.Empty }, Map);
        mapper.Define<CreateStylesheetRequestModel, StylesheetCreateModel>((_, _) => new StylesheetCreateModel { Name = string.Empty }, Map);
        mapper.Define<UpdateStylesheetRequestModel, StylesheetUpdateModel>((_, _) => new StylesheetUpdateModel { Content = string.Empty, Name = string.Empty, ExistingPath = string.Empty }, Map);
    }

    private void Map(UpdateStylesheetRequestModel source, StylesheetUpdateModel target, MapperContext context)
    {
        target.Content = source.Content;
        target.Name = source.Name;
        target.ExistingPath = source.ExistingPath;
    }

    private void Map(CreateStylesheetRequestModel source, StylesheetCreateModel target, MapperContext context)
    {
        target.Content = source.Content;
        target.Name = source.Name;
        target.ParentPath = source.ParentPath;
    }

    private void Map(IStylesheet source, StylesheetResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Content = source.Content ?? string.Empty;
        target.Path = source.Path;
    }
}
