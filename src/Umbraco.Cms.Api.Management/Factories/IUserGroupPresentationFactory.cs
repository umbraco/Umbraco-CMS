using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// A factory for creating <see cref="UserGroupResponseModel"/>
/// </summary>
public interface IUserGroupPresentationFactory
{
    /// <summary>
    /// Creates a <see cref="UserGroupResponseModel"/> based on a <see cref="UserGroup"/>
    /// </summary>
    /// <param name="userGroup"></param>
    /// <returns></returns>
    Task<UserGroupResponseModel> CreateAsync(IUserGroup userGroup);

    /// <summary>
    /// Creates multiple <see cref="UserGroupResponseModel"/> base on multiple <see cref="UserGroup"/>
    /// </summary>
    /// <param name="userGroups"></param>
    /// <returns></returns>
    Task<IEnumerable<UserGroupResponseModel>> CreateMultipleAsync(IEnumerable<IUserGroup> userGroups);

    /// <summary>
    /// Creates multiple <see cref="UserGroupResponseModel"/> base on multiple <see cref="UserGroup"/>
    /// </summary>
    /// <param name="userGroups"></param>
    /// <returns></returns>
    Task<IEnumerable<UserGroupResponseModel>> CreateMultipleAsync(IEnumerable<IReadOnlyUserGroup> userGroups);

    /// <summary>
    /// Creates an <see cref="IUserGroup"/> based on a <see cref="CreateUserGroupRequestModel"/>
    /// </summary>
    /// <param name="requestModel"></param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(CreateUserGroupRequestModel requestModel);

    /// <summary>
    /// Converts the values of an update model to fit with the existing backoffice implementations, and maps it to an existing user group.
    /// </summary>
    /// <param name="current">Existing user group to map to.</param>
    /// <param name="request">Update model containing the new values.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(IUserGroup current, UpdateUserGroupRequestModel request);
}
