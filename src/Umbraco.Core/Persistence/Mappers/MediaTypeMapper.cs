﻿using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="MediaType"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(IMediaType))]
    [MapperFor(typeof(MediaType))]
    public sealed class MediaTypeMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<MediaType, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<MediaType, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<MediaType, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<MediaType, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<MediaType, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<MediaType, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<MediaType, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<MediaType, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<MediaType, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<MediaType, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<MediaType, ContentTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<MediaType, ContentTypeDto>(src => src.AllowedAsRoot, dto => dto.AllowAtRoot);
            CacheMap<MediaType, ContentTypeDto>(src => src.Description, dto => dto.Description);
            CacheMap<MediaType, ContentTypeDto>(src => src.Icon, dto => dto.Icon);
            CacheMap<MediaType, ContentTypeDto>(src => src.IsContainer, dto => dto.IsContainer);
            CacheMap<MediaType, ContentTypeDto>(src => src.IsElement, dto => dto.IsElement);
            CacheMap<MediaType, ContentTypeDto>(src => src.Thumbnail, dto => dto.Thumbnail);
        }
    }
}
