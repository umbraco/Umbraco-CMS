using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(PublicAccessEntry))]
    public sealed class AccessMapper : BaseMapper
    {
        public AccessMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        {
            DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.Key), nameof(AccessDto.Id));
            DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.LoginNodeId), nameof(AccessDto.LoginNodeId));
            DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.NoAccessNodeId), nameof(AccessDto.NoAccessNodeId));
            DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.ProtectedNodeId), nameof(AccessDto.NodeId));
            DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.CreateDate), nameof(AccessDto.CreateDate));
            DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.UpdateDate), nameof(AccessDto.UpdateDate));
        }
    }
}
