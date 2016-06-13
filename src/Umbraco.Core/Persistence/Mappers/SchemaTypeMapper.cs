using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="SchemaType"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(ISchemaType))]
    [MapperFor(typeof(SchemaType))]
    public sealed class SchemaTypeMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public SchemaTypeMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty)
            {
                CacheMap<SchemaType, NodeDto>(src => src.Id, dto => dto.NodeId);
                CacheMap<SchemaType, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
                CacheMap<SchemaType, NodeDto>(src => src.Level, dto => dto.Level);
                CacheMap<SchemaType, NodeDto>(src => src.ParentId, dto => dto.ParentId);
                CacheMap<SchemaType, NodeDto>(src => src.Path, dto => dto.Path);
                CacheMap<SchemaType, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
                CacheMap<SchemaType, NodeDto>(src => src.Name, dto => dto.Text);
                CacheMap<SchemaType, NodeDto>(src => src.Trashed, dto => dto.Trashed);
                CacheMap<SchemaType, NodeDto>(src => src.Key, dto => dto.UniqueId);
                CacheMap<SchemaType, NodeDto>(src => src.CreatorId, dto => dto.UserId);
                CacheMap<SchemaType, ContentTypeDto>(src => src.Alias, dto => dto.Alias);
                CacheMap<SchemaType, ContentTypeDto>(src => src.AllowedAsRoot, dto => dto.AllowAtRoot);
                CacheMap<SchemaType, ContentTypeDto>(src => src.Description, dto => dto.Description);
                CacheMap<SchemaType, ContentTypeDto>(src => src.Icon, dto => dto.Icon);
                CacheMap<SchemaType, ContentTypeDto>(src => src.IsContainer, dto => dto.IsContainer);
                CacheMap<SchemaType, ContentTypeDto>(src => src.Thumbnail, dto => dto.Thumbnail);
            }
        }

        #endregion 
    }
}