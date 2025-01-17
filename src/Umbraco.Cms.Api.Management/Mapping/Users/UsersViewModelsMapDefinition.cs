using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
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
        mapper.Define<UserExternalLoginProviderModel, UserExternalLoginProviderResponseModel>(
            (_, _) => new UserExternalLoginProviderResponseModel { ProviderSchemeName = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(UserCreationResult source, CreateUserResponseModel target, MapperContext context)
    {
        Guid userKey = source.CreatedUser?.Key
                      ?? throw new ArgumentException("Cannot map a user creation response without a created user", nameof(source));

        target.User = new ReferenceByIdModel(userKey);
        target.InitialPassword = source.InitialPassword;
    }

    // Umbraco.Code.MapAll
    private void Map(PasswordChangedModel source, ResetPasswordUserResponseModel target, MapperContext context)
    {
        target.ResetPassword = source.ResetPassword;
    }

    // Umbraco.Code.MapAll
    private void Map(UserExternalLoginProviderModel source, UserExternalLoginProviderResponseModel target, MapperContext context)
    {
        target.ProviderSchemeName = source.ProviderSchemeName;
        target.HasManualLinkingEnabled = source.HasManualLinkingEnabled;
        target.IsLinkedOnUser = source.IsLinkedOnUser;
        target.ProviderKey = source.ProviderKey;
    }
}
