using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// A factory for creating <see cref="IPermissionPresentationModel"/>
/// </summary>
public interface IPermissionPresentationFactory
{
    Task<ISet<IPermissionPresentationModel>> CreateAsync(ISet<IGranularPermission> userGroupGranularPermissions);
    Task<ISet<IGranularPermission>> CreatePermissionSetsAsync(ISet<IPermissionPresentationModel> permissions);
}
