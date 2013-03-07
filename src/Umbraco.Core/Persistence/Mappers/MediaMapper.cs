using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Models.Media"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    internal sealed class MediaMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public MediaMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty)
            {
                CacheMap<Models.Media, NodeDto>(src => src.Id, dto => dto.NodeId);
                CacheMap<Models.Media, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
                CacheMap<Models.Media, NodeDto>(src => src.Level, dto => dto.Level);
                CacheMap<Models.Media, NodeDto>(src => src.ParentId, dto => dto.ParentId);
                CacheMap<Models.Media, NodeDto>(src => src.Path, dto => dto.Path);
                CacheMap<Models.Media, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
                CacheMap<Models.Media, NodeDto>(src => src.Name, dto => dto.Text);
                CacheMap<Models.Media, NodeDto>(src => src.Trashed, dto => dto.Trashed);
                CacheMap<Models.Media, NodeDto>(src => src.Key, dto => dto.UniqueId);
                CacheMap<Models.Media, NodeDto>(src => src.CreatorId, dto => dto.UserId);
                CacheMap<Models.Media, ContentDto>(src => src.ContentTypeId, dto => dto.ContentTypeId);
                CacheMap<Models.Media, ContentVersionDto>(src => src.UpdateDate, dto => dto.VersionDate);
                CacheMap<Models.Media, ContentVersionDto>(src => src.Version, dto => dto.VersionId);
            }
        }

        internal override string Map(string propertyName)
        {
            if (!PropertyInfoCache.ContainsKey(propertyName))
                return string.Empty;

            var dtoTypeProperty = PropertyInfoCache[propertyName];

            return base.GetColumnName(dtoTypeProperty.Type, dtoTypeProperty.PropertyInfo);
        }

        internal override void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var property = base.ResolveMapping(sourceMember, destinationMember);
            PropertyInfoCache.AddOrUpdate(property.SourcePropertyName, property, (x, y) => property);
        }

        #endregion
    }
}