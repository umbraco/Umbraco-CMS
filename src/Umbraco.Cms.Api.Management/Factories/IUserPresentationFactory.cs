using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IUserPresentationFactory
{
    UserResponseModel CreateResponseModel(IUser user);

    Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel);

    Task<UserInviteModel> CreateInviteModelAsync(InviteUserRequestModel requestModel);

    Task<UserUpdateModel> CreateUpdateModelAsync(Guid existingUserKey, UpdateUserRequestModel updateModel);

    Task<CurrentUserResponseModel> CreateCurrentUserResponseModelAsync(IUser user);

    Task<UserResendInviteModel> CreateResendInviteModelAsync(ResendInviteUserRequestModel requestModel);

    Task<UserConfigurationResponseModel> CreateUserConfigurationModelAsync();

    Task<CurrentUserConfigurationResponseModel> CreateCurrentUserConfigurationModelAsync();

    UserItemResponseModel CreateItemResponseModel(IUser user);

    Task<CalculatedUserStartNodesResponseModel> CreateCalculatedUserStartNodesResponseModelAsync(IUser user);
}
