using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Folder;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Stylesheet;

public class StylesheetViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IStylesheet, StylesheetResponseModel>((_, _) => new StylesheetResponseModel { Content = string.Empty, Name = string.Empty, Path = string.Empty }, Map);
        mapper.Define<CreateStylesheetRequestModel, StylesheetCreateModel>((_, _) => new StylesheetCreateModel { Name = string.Empty }, Map);
        mapper.Define<UpdateStylesheetRequestModel, StylesheetUpdateModel>((_, _) => new StylesheetUpdateModel { Content = string.Empty }, Map);
        mapper.Define<RenameStylesheetRequestModel, StylesheetRenameModel>((_, _) => new StylesheetRenameModel { Name = string.Empty }, Map);

        mapper.Define<StylesheetFolderModel, StylesheetFolderResponseModel>((_, _) => new StylesheetFolderResponseModel { Name = string.Empty, Path = string.Empty }, Map);
        mapper.Define<CreateStylesheetFolderRequestModel, StylesheetFolderCreateModel>((_, _) => new StylesheetFolderCreateModel { Name = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IStylesheet source, StylesheetResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Content = source.Content ?? string.Empty;
        target.Path = source.Path.SystemPathToVirtualPath();
        var parentPath = Path.GetDirectoryName(source.Path);
        target.Parent = parentPath.IsNullOrWhiteSpace()
            ? null
            : new FileSystemFolderModel
            {
                Path = parentPath.SystemPathToVirtualPath()
            };
    }

    // Umbraco.Code.MapAll
    private void Map(CreateStylesheetRequestModel source, StylesheetCreateModel target, MapperContext context)
    {
        target.Content = source.Content;
        target.Name = source.Name;
        target.ParentPath = source.Parent?.Path.VirtualPathToSystemPath();
    }

    // Umbraco.Code.MapAll
    private void Map(UpdateStylesheetRequestModel source, StylesheetUpdateModel target, MapperContext context)
        => target.Content = source.Content;

    // Umbraco.Code.MapAll
    private void Map(RenameStylesheetRequestModel source, StylesheetRenameModel target, MapperContext context)
        => target.Name = source.Name;

    // Umbraco.Code.MapAll
    private void Map(StylesheetFolderModel source, StylesheetFolderResponseModel target, MapperContext context)
    {
        target.Path = source.Path.SystemPathToVirtualPath();
        target.Name = source.Name;
        target.Parent = string.IsNullOrEmpty(source.ParentPath)
            ? null
            : new FileSystemFolderModel
            {
                Path = source.ParentPath!.SystemPathToVirtualPath()
            };
    }

    // Umbraco.Code.MapAll
    private void Map(CreateStylesheetFolderRequestModel source, StylesheetFolderCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.ParentPath = source.Parent?.Path.VirtualPathToSystemPath();
    }
}
