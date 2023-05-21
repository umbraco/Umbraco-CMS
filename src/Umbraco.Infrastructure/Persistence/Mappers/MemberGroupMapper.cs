using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(IMemberGroup))]
[MapperFor(typeof(MemberGroup))]
public sealed class MemberGroupMapper : BaseMapper
{
    public MemberGroupMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.Id), nameof(NodeDto.NodeId));
        DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.CreateDate), nameof(NodeDto.CreateDate));
        DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.CreatorId), nameof(NodeDto.UserId));
        DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.Name), nameof(NodeDto.Text));
        DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.Key), nameof(NodeDto.UniqueId));
    }
}
