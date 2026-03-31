using Umbraco.Cms.Api.Management.ViewModels.UserData;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Mapping.UserData;

/// <summary>
/// Provides mapping configuration for transforming user data objects within the API management layer.
/// </summary>
public class UserDataMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures object-object mappings related to user data, including mappings between user data models and API request/response models.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mappings.</param>
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IUserData, UserDataResponseModel>((_, _) => new UserDataResponseModel(), Map);
        mapper.Define<CreateUserDataRequestModel, IUserData>((_, _) => new Core.Models.Membership.UserData(), Map);
        mapper.Define<UpdateUserDataRequestModel, IUserData>((_, _) => new Core.Models.Membership.UserData(), Map);
    }

    private void Map(IUserData source, UserDataResponseModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Group = source.Group;
        target.Identifier = source.Identifier;
        target.Value = source.Value;
    }

    private void Map(CreateUserDataRequestModel source, IUserData target, MapperContext context)
    {
        target.Key = source.Key ?? Guid.NewGuid();
        MapBase(source, target, context);
    }

    private void Map(UpdateUserDataRequestModel source, IUserData target, MapperContext context)
    {
        target.Key = source.Key;
        MapBase(source, target, context);
    }

    private void MapBase(UserDataViewModel source, IUserData target, MapperContext context)
    {
        target.Group = source.Group;
        target.Identifier = source.Identifier;
        target.Value = source.Value;
    }
}
