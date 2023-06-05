using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Mapping.Users;

public class UsersViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<PasswordChangedModel, ChangePasswordUserResponseModel>((_, _) => new ChangePasswordUserResponseModel(), Map);
        mapper.Define<UserCreationResult, CreateUserResponseModel>((_, _) => new CreateUserResponseModel(), Map);
        mapper.Define<IIdentityUserLogin, LinkedLoginViewModel>((_, _) => new LinkedLoginViewModel { ProviderKey = string.Empty, ProviderName = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IIdentityUserLogin source, LinkedLoginViewModel target, MapperContext context)
    {
        target.ProviderKey = source.ProviderKey;
        target.ProviderName = source.LoginProvider;
    }

    // Umbraco.Code.MapAll
    private void Map(UserCreationResult source, CreateUserResponseModel target, MapperContext context)
    {
        target.UserId = source.CreatedUser?.Key ?? Guid.Empty;
        target.InitialPassword = source.InitialPassword;
    }

    // Umbraco.Code.MapAll
    private void Map(PasswordChangedModel source, ChangePasswordUserResponseModel target, MapperContext context)
    {
        target.ResetPassword = source.ResetPassword;
    }
}
