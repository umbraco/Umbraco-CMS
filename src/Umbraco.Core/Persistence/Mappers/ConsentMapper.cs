using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a mapper for consent entities.
    /// </summary>
    [MapperFor(typeof(IConsent))]
    [MapperFor(typeof(Consent))]
    public sealed class ConsentMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance
            = new ConcurrentDictionary<string, DtoMapModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentMapper"/> class.
        /// </summary>
        public ConsentMapper()
        {
            // note: why the base ctor does not invoke BuildMap is a mystery to me
            BuildMap();
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        internal override void BuildMap()
        {
            CacheMap<Consent, ConsentDto>(entity => entity.Id, dto => dto.Id);
            CacheMap<Consent, ConsentDto>(entity => entity.Current, dto => dto.Current);
            CacheMap<Consent, ConsentDto>(entity => entity.CreateDate, dto => dto.CreateDate);
            CacheMap<Consent, ConsentDto>(entity => entity.Source, dto => dto.Source);
            CacheMap<Consent, ConsentDto>(entity => entity.Context, dto => dto.Context);
            CacheMap<Consent, ConsentDto>(entity => entity.Action, dto => dto.Action);
            CacheMap<Consent, ConsentDto>(entity => entity.State, dto => dto.State);
            CacheMap<Consent, ConsentDto>(entity => entity.Comment, dto => dto.Comment);
        }
    }
}
