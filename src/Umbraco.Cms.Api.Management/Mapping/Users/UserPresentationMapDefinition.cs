using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Users;

public class UserPresentationMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper) {
        mapper.Define<CreateUserRequestModel, UserCreateModel>((_, _ ) => new UserCreateModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(CreateUserRequestModel source, UserCreateModel target, MapperContext context)
    {
        target.Email = source.Email;
        target.UserName = source.UserName;
        target.UserGroups = source.UserGroups;
        target.Name = source.Name;
    }
}
