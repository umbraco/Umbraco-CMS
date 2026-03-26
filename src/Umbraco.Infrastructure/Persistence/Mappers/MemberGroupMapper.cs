using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Maps <see cref="MemberGroup"/> entities between the database and the domain model.
/// </summary>
[MapperFor(typeof(IMemberGroup))]
[MapperFor(typeof(MemberGroup))]
public sealed class MemberGroupMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberGroupMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">A lazily initialized SQL context used for database operations.</param>
    /// <param name="maps">The configuration store containing mapping definitions.</param>
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
