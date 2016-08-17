using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="MemberType"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof (MemberType))]
    [MapperFor(typeof (IMemberType))]
    public sealed class MemberTypeMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<MemberType, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<MemberType, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<MemberType, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<MemberType, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<MemberType, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<MemberType, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<MemberType, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<MemberType, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<MemberType, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<MemberType, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<MemberType, ContentTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<MemberType, ContentTypeDto>(src => src.AllowedAsRoot, dto => dto.AllowAtRoot);
            CacheMap<MemberType, ContentTypeDto>(src => src.Description, dto => dto.Description);
            CacheMap<MemberType, ContentTypeDto>(src => src.Icon, dto => dto.Icon);
            CacheMap<MemberType, ContentTypeDto>(src => src.IsContainer, dto => dto.IsContainer);
            CacheMap<MemberType, ContentTypeDto>(src => src.Thumbnail, dto => dto.Thumbnail);
        }
    }
}