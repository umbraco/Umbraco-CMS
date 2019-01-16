using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="ContentType"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(ContentType))]
    [MapperFor(typeof(IContentType))]
    public sealed class ContentTypeMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<ContentType, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<ContentType, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<ContentType, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<ContentType, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<ContentType, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<ContentType, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<ContentType, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<ContentType, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<ContentType, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<ContentType, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<ContentType, ContentTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<ContentType, ContentTypeDto>(src => src.AllowedAsRoot, dto => dto.AllowAtRoot);
            CacheMap<ContentType, ContentTypeDto>(src => src.Description, dto => dto.Description);
            CacheMap<ContentType, ContentTypeDto>(src => src.Icon, dto => dto.Icon);
            CacheMap<ContentType, ContentTypeDto>(src => src.IsContainer, dto => dto.IsContainer);
            CacheMap<ContentType, ContentTypeDto>(src => src.IsElement, dto => dto.IsElement);
            CacheMap<ContentType, ContentTypeDto>(src => src.Thumbnail, dto => dto.Thumbnail);
        }
    }
}
