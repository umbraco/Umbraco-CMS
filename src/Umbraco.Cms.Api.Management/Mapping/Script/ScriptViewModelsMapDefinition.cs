using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Script;
using Umbraco.Cms.Api.Management.ViewModels.Script.Folder;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Script;

public class ScriptViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IScript, ScriptResponseModel>((_, _) => new ScriptResponseModel { Name = string.Empty, Path = string.Empty, Content = string.Empty }, Map);
        mapper.Define<CreateScriptRequestModel, ScriptCreateModel>((_, _) => new ScriptCreateModel { Name = string.Empty }, Map);
        mapper.Define<UpdateScriptRequestModel, ScriptUpdateModel>((_, _) => new ScriptUpdateModel { Content = string.Empty }, Map);
        mapper.Define<RenameScriptRequestModel, ScriptRenameModel>((_, _) => new ScriptRenameModel { Name = string.Empty }, Map);

        mapper.Define<ScriptFolderModel, ScriptFolderResponseModel>((_, _) => new ScriptFolderResponseModel { Name = string.Empty, Path = string.Empty }, Map);
        mapper.Define<CreateScriptFolderRequestModel, ScriptFolderCreateModel>((_, _) => new ScriptFolderCreateModel { Name = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IScript source, ScriptResponseModel target, MapperContext context)
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
    private void Map(CreateScriptRequestModel source, ScriptCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Content = source.Content;
        target.ParentPath = source.Parent?.Path.VirtualPathToSystemPath();
    }

    // Umbraco.Code.MapAll
    private void Map(UpdateScriptRequestModel source, ScriptUpdateModel target, MapperContext context)
        => target.Content = source.Content;

    // Umbraco.Code.MapAll
    private void Map(RenameScriptRequestModel source, ScriptRenameModel target, MapperContext context)
        => target.Name = source.Name;

    // Umbraco.Code.MapAll
    private void Map(ScriptFolderModel source, ScriptFolderResponseModel target, MapperContext context)
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
    private void Map(CreateScriptFolderRequestModel source, ScriptFolderCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.ParentPath = source.Parent?.Path.VirtualPathToSystemPath();
    }
}
