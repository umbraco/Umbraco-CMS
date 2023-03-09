using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IUserPresentationFactory
{
    UserResponseModel CreateResponseModel(IUser user);

    Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel);

    Task<UserInviteModel> CreateInviteModelAsync(InviteUserRequestModel requestModel);

    Task<UserUpdateModel> CreateUpdateModelAsync(IUser existingUser, UpdateUserRequestModel updateModel);

    CreateUserResponseModel CreateCreationResponseModel(UserCreationResult creationResult);
}
