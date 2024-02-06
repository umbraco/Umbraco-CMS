using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.MemberGroup;

public class MemberGroupMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<CreateMemberGroupRequestModel, IMemberGroup>((_, _) => new Core.Models.MemberGroup(), Map);
        mapper.Define<UpdateMemberGroupRequestModel, IMemberGroup>((_, _) => new Core.Models.MemberGroup(), Map);
        mapper.Define<IMemberGroup, MemberGroupResponseModel>((_, _) => new MemberGroupResponseModel { Name = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll -Id -CreateDate -CreatorId -DeleteDate -UpdateDate
    private static void Map(CreateMemberGroupRequestModel source, IMemberGroup target, MapperContext context)
    {
        target.Name = source.Name;
        target.Key = source.Id ?? Guid.NewGuid();
    }

    // Umbraco.Code.MapAll -Id -CreateDate -CreatorId -DeleteDate -UpdateDate -Key
    private static void Map(UpdateMemberGroupRequestModel source, IMemberGroup target, MapperContext context) => target.Name = source.Name;

    // Umbraco.Code.MapAll
    private static void Map(IMemberGroup source, MemberGroupResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
    }
}
