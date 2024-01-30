using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.MemberGroup;

public class MemberGroupMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<CreateMemberGroupRequestModel, Core.Models.IMemberGroup>((_, _) => new Core.Models.MemberGroup(), Map);

    // Umbraco.Code.MapAll -Id -CreateDate -CreatorId -DeleteDate -UpdateDate
    private static void Map(CreateMemberGroupRequestModel source, Core.Models.IMemberGroup target, MapperContext context)
    {
        target.Name = source.Name;
        target.Key = source.Id ?? Guid.NewGuid();
    }
}
