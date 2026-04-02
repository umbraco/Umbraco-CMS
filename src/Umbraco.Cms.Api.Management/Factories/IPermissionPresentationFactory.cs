using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// A factory for creating <see cref="IPermissionPresentationModel"/>
/// </summary>
public interface IPermissionPresentationFactory
{
    /// <summary>
    /// Creates a set of permission presentation models based on the provided granular permissions.
    /// </summary>
    /// <param name="userGroupGranularPermissions">The set of granular permissions for the user group.</param>
    /// <returns>A task representing the asynchronous operation, containing a set of permission presentation models.</returns>
    Task<ISet<IPermissionPresentationModel>> CreateAsync(ISet<IGranularPermission> userGroupGranularPermissions);
    /// <summary>
    /// Creates a set of granular permissions from the given permission presentation models.
    /// </summary>
    /// <param name="permissions">The set of permission presentation models to convert.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a set of granular permissions.</returns>
    Task<ISet<IGranularPermission>> CreatePermissionSetsAsync(ISet<IPermissionPresentationModel> permissions);
}
