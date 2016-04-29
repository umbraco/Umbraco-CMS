using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Content"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(Content))]
    [MapperFor(typeof(IContent))]
    public sealed class ContentMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<Content, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<Content, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<Content, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<Content, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<Content, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<Content, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<Content, NodeDto>(src => src.NodeName, dto => dto.Text);
            CacheMap<Content, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<Content, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<Content, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<Content, ContentDto>(src => src.ContentTypeId, dto => dto.ContentTypeId);
            CacheMap<Content, ContentVersionDto>(src => src.UpdateDate, dto => dto.VersionDate);
            CacheMap<Content, ContentVersionDto>(src => src.Version, dto => dto.VersionId);
            CacheMap<Content, DocumentDto>(src => src.Name, dto => dto.Text);
            CacheMap<Content, DocumentDto>(src => src.ExpireDate, dto => dto.ExpiresDate);
            CacheMap<Content, DocumentDto>(src => src.ReleaseDate, dto => dto.ReleaseDate);
            CacheMap<Content, DocumentDto>(src => src.Published, dto => dto.Published);
            //CacheMap<Content, DocumentDto>(src => src.Name, dto => dto.Alias);
            //CacheMap<Content, DocumentDto>(src => src, dto => dto.Newest);
            //CacheMap<Content, DocumentDto>(src => src.Template, dto => dto.TemplateId);
        }
    }
}