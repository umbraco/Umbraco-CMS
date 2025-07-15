using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines factory methods for the creation of user presentation models.
/// </summary>
public interface IUserPresentationFactory
{
    /// <summary>
    /// Creates a response model for the provided user.
    /// </summary>
    UserResponseModel CreateResponseModel(IUser user);

    /// <summary>
    /// Creates a create model for a user based on the provided request model.
    /// </summary>
    Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel);

    /// <summary>
    /// Creates an invite model for a user based on the provided request model.
    /// </summary>
    Task<UserInviteModel> CreateInviteModelAsync(InviteUserRequestModel requestModel);

    /// <summary>
    /// Creates an update model for an existing user based on the provided request model.
    /// </summary>
    Task<UserUpdateModel> CreateUpdateModelAsync(Guid existingUserKey, UpdateUserRequestModel updateModel);

    /// <summary>
    /// Creates a response model for the current user based on the provided user.
    /// </summary>
    Task<CurrentUserResponseModel> CreateCurrentUserResponseModelAsync(IUser user);

    /// <summary>
    /// Creates an resend invite model for a user based on the provided request model.
    /// </summary>
    Task<UserResendInviteModel> CreateResendInviteModelAsync(ResendInviteUserRequestModel requestModel);

    /// <summary>
    /// Creates a user configuration model that contains the necessary data for user management operations.
    /// </summary>
    Task<UserConfigurationResponseModel> CreateUserConfigurationModelAsync();

    /// <summary>
    /// Creates a current user configuration model that contains the necessary data for the current user's management operations.
    /// </summary>
    Task<CurrentUserConfigurationResponseModel> CreateCurrentUserConfigurationModelAsync();

    /// <summary>
    /// Creates a user item response model for the provided user.
    /// </summary>
    UserItemResponseModel CreateItemResponseModel(IUser user);

    /// <summary>
    /// Creates a calculated user start nodes response model based on the provided user.
    /// </summary>
    Task<CalculatedUserStartNodesResponseModel> CreateCalculatedUserStartNodesResponseModelAsync(IUser user);
}
