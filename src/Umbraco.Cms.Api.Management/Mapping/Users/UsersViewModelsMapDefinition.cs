using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Mapping.Users;

public class UsersViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<PasswordChangedModel, ChangePasswordUserResponseModel>((_, _) => new ChangePasswordUserResponseModel(), Map);
        mapper.Define<UserCreationResult, CreateUserResponseModel>((_, _) => new CreateUserResponseModel(), Map);
    }

    private void Map(UserCreationResult source, CreateUserResponseModel target, MapperContext context)
    {
        target.UserId = source.CreatedUser?.Key ?? Guid.Empty;
        target.InitialPassword = source.InitialPassword;
    }

    private void Map(PasswordChangedModel source, ChangePasswordUserResponseModel target, MapperContext context)
    {
        target.ResetPassword = source.ResetPassword;
    }
}
