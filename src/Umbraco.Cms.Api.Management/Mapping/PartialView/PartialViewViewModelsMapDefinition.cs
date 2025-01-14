using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Folder;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Snippets;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.PartialView;

public class PartialViewViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IPartialView, PartialViewResponseModel>((_, _) => new PartialViewResponseModel { Name = string.Empty, Path = string.Empty, Content = string.Empty }, Map);
        mapper.Define<CreatePartialViewRequestModel, PartialViewCreateModel>((_, _) => new PartialViewCreateModel { Name = string.Empty }, Map);
        mapper.Define<UpdatePartialViewRequestModel, PartialViewUpdateModel>((_, _) => new PartialViewUpdateModel { Content = string.Empty }, Map);
        mapper.Define<RenamePartialViewRequestModel, PartialViewRenameModel>((_, _) => new PartialViewRenameModel { Name = string.Empty }, Map);

        mapper.Define<PartialViewSnippet, PartialViewSnippetResponseModel>((_, _) => new PartialViewSnippetResponseModel { Id = string.Empty, Name = string.Empty, Content = string.Empty }, Map);
        mapper.Define<PartialViewSnippetSlim, PartialViewSnippetItemResponseModel>((_, _) => new PartialViewSnippetItemResponseModel { Id = string.Empty, Name = string.Empty }, Map);

        mapper.Define<PartialViewFolderModel, PartialViewFolderResponseModel>((_, _) => new PartialViewFolderResponseModel { Name = string.Empty, Path = string.Empty }, Map);
        mapper.Define<CreatePartialViewFolderRequestModel, PartialViewFolderCreateModel>((_, _) => new PartialViewFolderCreateModel { Name = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IPartialView source, PartialViewResponseModel target, MapperContext context)
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
    private void Map(CreatePartialViewRequestModel source, PartialViewCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Content = source.Content;
        target.ParentPath = source.Parent?.Path.VirtualPathToSystemPath();
    }

    // Umbraco.Code.MapAll
    private void Map(UpdatePartialViewRequestModel source, PartialViewUpdateModel target, MapperContext context)
        => target.Content = source.Content;

    // Umbraco.Code.MapAll
    private void Map(RenamePartialViewRequestModel source, PartialViewRenameModel target, MapperContext context)
        => target.Name = source.Name;

    // Umbraco.Code.MapAll
    private void Map(PartialViewFolderModel source, PartialViewFolderResponseModel target, MapperContext context)
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
    private void Map(PartialViewSnippet source, PartialViewSnippetResponseModel target, MapperContext context)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Content = source.Content;
    }

    // Umbraco.Code.MapAll
    private void Map(PartialViewSnippetSlim source, PartialViewSnippetItemResponseModel target, MapperContext context)
    {
        target.Id = source.Id;
        target.Name = source.Name;
    }

    // Umbraco.Code.MapAll
    private void Map(CreatePartialViewFolderRequestModel source, PartialViewFolderCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.ParentPath = source.Parent?.Path.VirtualPathToSystemPath();
    }
}
