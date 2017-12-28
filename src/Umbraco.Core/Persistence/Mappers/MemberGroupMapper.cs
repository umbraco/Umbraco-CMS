using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof (IMemberGroup))]
    [MapperFor(typeof (MemberGroup))]
    public sealed class MemberGroupMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<MemberGroup, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<MemberGroup, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<MemberGroup, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<MemberGroup, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<MemberGroup, NodeDto>(src => src.Key, dto => dto.UniqueId);
        }
    }
}
