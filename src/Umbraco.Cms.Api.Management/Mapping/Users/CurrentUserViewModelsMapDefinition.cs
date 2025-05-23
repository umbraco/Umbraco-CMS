using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Users;

public class CurrentUserViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<NodePermissions, UserPermissionViewModel>((_, _) => new UserPermissionViewModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(NodePermissions source, UserPermissionViewModel target, MapperContext context)
    {
        target.NodeKey = source.NodeKey;
        target.Permissions = source.Permissions;
    }
}
