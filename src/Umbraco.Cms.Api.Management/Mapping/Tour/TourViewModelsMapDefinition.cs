using Umbraco.Cms.Api.Management.ViewModels.Tour;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Tour;

public class TourViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<UserTourStatus, TourStatusViewModel>((_, _) => new TourStatusViewModel{ Alias = string.Empty}, Map);
        mapper.Define<SetTourStatusRequestModel, UserTourStatus>((_, _) => new UserTourStatus(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(SetTourStatusRequestModel source, UserTourStatus target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Completed = source.Completed;
        target.Disabled = source.Disabled;
    }

    // Umbraco.Code.MapAll
    private void Map(UserTourStatus source, TourStatusViewModel target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Completed = source.Completed;
        target.Disabled = source.Disabled;
    }
}
