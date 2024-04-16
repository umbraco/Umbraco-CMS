using Umbraco.Cms.Api.Management.ViewModels;
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
        mapper.Define<PasswordChangedModel, ResetPasswordUserResponseModel>((_, _) => new ResetPasswordUserResponseModel(), Map);
        mapper.Define<UserCreationResult, CreateUserResponseModel>((_, _) => new CreateUserResponseModel { User = new() }, Map);
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
        Guid userId = source.CreatedUser?.Key
                      ?? throw new ArgumentException("Cannot map a user creation response without a created user", nameof(source));

        target.User = new ReferenceByIdModel(userId);
        target.InitialPassword = source.InitialPassword;
    }

    // Umbraco.Code.MapAll
    private void Map(PasswordChangedModel source, ResetPasswordUserResponseModel target, MapperContext context)
    {
        target.ResetPassword = source.ResetPassword;
    }
}
