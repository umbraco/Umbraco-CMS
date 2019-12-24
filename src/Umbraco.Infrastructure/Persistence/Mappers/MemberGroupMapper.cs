using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof (IMemberGroup))]
    [MapperFor(typeof (MemberGroup))]
    public sealed class MemberGroupMapper : BaseMapper
    {
        public MemberGroupMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.Id), nameof(NodeDto.NodeId));
            DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.CreateDate), nameof(NodeDto.CreateDate));
            DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.CreatorId), nameof(NodeDto.UserId));
            DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.Name), nameof(NodeDto.Text));
            DefineMap<MemberGroup, NodeDto>(nameof(MemberGroup.Key), nameof(NodeDto.UniqueId));
        }
    }
}
