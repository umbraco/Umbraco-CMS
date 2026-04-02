using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Users;

/// <summary>
/// Provides mapping configuration for view models related to the current user in the Umbraco CMS Management API.
/// </summary>
public class CurrentUserViewModelsMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures the mapping between <see cref="NodePermissions"/> and <see cref="UserPermissionViewModel"/> for the current user context.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mapping.</param>
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<NodePermissions, UserPermissionViewModel>((_, _) => new UserPermissionViewModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(NodePermissions source, UserPermissionViewModel target, MapperContext context)
    {
        target.NodeKey = source.NodeKey;
        target.Permissions = source.Permissions;
    }
}
