using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(MigrationEntry))]
    [MapperFor(typeof(IMigrationEntry))]
    internal sealed class MigrationEntryMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public MigrationEntryMapper()
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
            CacheMap<MigrationEntry, MigrationDto>(src => src.Id, dto => dto.Id);            
            CacheMap<MigrationEntry, MigrationDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<MigrationEntry, MigrationDto>(src => src.UpdateDate, dto => dto.CreateDate);
            CacheMap<MigrationEntry, MigrationDto>(src => src.Version, dto => dto.Version);
            CacheMap<MigrationEntry, MigrationDto>(src => src.MigrationName, dto => dto.Name);
        }

        #endregion
    }
}