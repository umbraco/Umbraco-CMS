using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// A factory for creating <see cref="UserGroupViewModel"/>
/// </summary>
public interface IUserGroupViewModelFactory
{
    /// <summary>
    /// Creates a <see cref="UserGroupViewModel"/> based on a <see cref="UserGroup"/>
    /// </summary>
    /// <param name="userGroup"></param>
    /// <returns></returns>
    Task<UserGroupViewModel> CreateAsync(IUserGroup userGroup);

    /// <summary>
    /// Creates multiple <see cref="UserGroupViewModel"/> base on multiple <see cref="UserGroup"/>
    /// </summary>
    /// <param name="userGroups"></param>
    /// <returns></returns>
    Task<IEnumerable<UserGroupViewModel>> CreateMultipleAsync(IEnumerable<IUserGroup> userGroups);

    /// <summary>
    /// Creates an <see cref="IUserGroup"/> based on a <see cref="UserGroupSaveModel"/>
    /// </summary>
    /// <param name="saveModel"></param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(UserGroupSaveModel saveModel);

    /// <summary>
    /// Converts the values of an update model to fit with the existing backoffice implementations, and maps it to an existing user group.
    /// </summary>
    /// <param name="current">Existing user group to map to.</param>
    /// <param name="update">Update model containing the new values.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(IUserGroup current, UserGroupUpdateModel update);
}
