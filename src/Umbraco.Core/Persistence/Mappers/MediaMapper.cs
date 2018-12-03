using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Models.Media"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(IMedia))]
    [MapperFor(typeof(Umbraco.Core.Models.Media))]
    public sealed class MediaMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<Models.Media, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<Models.Media, NodeDto>(src => src.Key, dto => dto.UniqueId);

            CacheMap<Content, ContentVersionDto>(src => src.VersionId, dto => dto.Id);

            CacheMap<Models.Media, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<Models.Media, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<Models.Media, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<Models.Media, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<Models.Media, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<Models.Media, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<Models.Media, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<Models.Media, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<Models.Media, ContentDto>(src => src.ContentTypeId, dto => dto.ContentTypeId);
            CacheMap<Models.Media, ContentVersionDto>(src => src.UpdateDate, dto => dto.VersionDate);
        }
    }
}
