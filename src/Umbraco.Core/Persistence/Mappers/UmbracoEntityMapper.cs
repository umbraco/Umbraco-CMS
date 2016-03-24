using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof (IUmbracoEntity))]
    public sealed class UmbracoEntityMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();


        #region Overrides of BaseMapper

        public UmbracoEntityMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
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