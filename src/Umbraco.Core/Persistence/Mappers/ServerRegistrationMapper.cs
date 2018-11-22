using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(ServerRegistration))]
    [MapperFor(typeof(IServerRegistration))]
    internal sealed class ServerRegistrationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.Id, dto => dto.Id);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.IsActive, dto => dto.IsActive);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.IsMaster, dto => dto.IsMaster);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.ServerAddress, dto => dto.ServerAddress);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.CreateDate, dto => dto.DateRegistered);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.UpdateDate, dto => dto.DateAccessed);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.ServerIdentity, dto => dto.ServerIdentity);
        }
    }
}
