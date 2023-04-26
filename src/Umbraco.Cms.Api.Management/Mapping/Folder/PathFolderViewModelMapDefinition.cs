using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Folder;

public class PathFolderViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<PathContainer, PathFolderResponseModel>((_, _) => new PathFolderResponseModel(), Map);
        mapper.Define<CreatePathFolderRequestModel, PathContainer>((_, _) => new PathContainer { Name = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(CreatePathFolderRequestModel source, PathContainer target, MapperContext context)
    {
        target.Name = source.Name;
        target.ParentPath = source.ParentPath;
    }

    // Umbraco.Code.MapAll
    private void Map(PathContainer source, PathFolderResponseModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.ParentPath = source.ParentPath;
    }
}
