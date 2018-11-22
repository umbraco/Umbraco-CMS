using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof (IUmbracoEntity))]
    public sealed class UmbracoEntityMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public UmbracoEntityMapper()
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
            CacheMap<IUmbracoEntity, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<IUmbracoEntity, NodeDto>(src => src.CreatorId, dto => dto.UserId);
        }
        
        #endregion
    }
}