using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Users;

public class CurrentUserViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<UserData, UserDataViewModel>((_, _) => new UserDataViewModel {Data = string.Empty, Name = string.Empty }, Map);
    }

    private void Map(UserData source, UserDataViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Data = source.Data;
    }
}
