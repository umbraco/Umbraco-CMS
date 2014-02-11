using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof (IMemberGroup))]
    [MapperFor(typeof (MemberGroup))]
    public sealed class MemberGroupMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public MemberGroupMapper()
        {
            BuildMap();
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
        {
            CacheMap<MemberGroup, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<MemberGroup, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<MemberGroup, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<MemberGroup, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<MemberGroup, NodeDto>(src => src.Key, dto => dto.UniqueId);
        }
    }
}