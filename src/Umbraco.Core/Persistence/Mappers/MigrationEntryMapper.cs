using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(MigrationEntry))]
    [MapperFor(typeof(IMigrationEntry))]
    internal sealed class MigrationEntryMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        public MigrationEntryMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
        {
            CacheMap<MigrationEntry, MigrationDto>(src => src.Id, dto => dto.Id);            
            CacheMap<MigrationEntry, MigrationDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<MigrationEntry, MigrationDto>(src => src.UpdateDate, dto => dto.CreateDate);
            CacheMap<MigrationEntry, MigrationDto>(src => src.Version, dto => dto.Version);
            CacheMap<MigrationEntry, MigrationDto>(src => src.MigrationName, dto => dto.Name);
        }

        #endregion

        
    }
}