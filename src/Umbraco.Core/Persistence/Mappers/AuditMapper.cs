using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(AuditItem))]
    [MapperFor(typeof(IAuditItem))]
    public sealed class AuditMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        public AuditMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {

        }

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public AuditMapper()
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
                CacheMap<AuditItem, LogDto>(src => src.Id, dto => dto.NodeId);
                CacheMap<AuditItem, LogDto>(src => src.CreateDate, dto => dto.Datestamp);
                CacheMap<AuditItem, LogDto>(src => src.UserId, dto => dto.UserId);
                CacheMap<AuditItem, LogDto>(src => src.AuditType, dto => dto.Header);                
                CacheMap<AuditItem, LogDto>(src => src.Comment, dto => dto.Comment);
            }
        }

        #endregion
    }
}