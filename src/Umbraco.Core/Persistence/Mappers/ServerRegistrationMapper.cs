using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(ServerRegistration))]
    [MapperFor(typeof(IServerRegistration))]
    internal sealed class ServerRegistrationMapper : BaseMapper
    {
        public ServerRegistrationMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        {
            DefineMap<ServerRegistration, ServerRegistrationDto>(nameof(ServerRegistration.Id), nameof(ServerRegistrationDto.Id));
            DefineMap<ServerRegistration, ServerRegistrationDto>(nameof(ServerRegistration.IsActive), nameof(ServerRegistrationDto.IsActive));
            DefineMap<ServerRegistration, ServerRegistrationDto>(nameof(ServerRegistration.IsMaster), nameof(ServerRegistrationDto.IsMaster));
            DefineMap<ServerRegistration, ServerRegistrationDto>(nameof(ServerRegistration.ServerAddress), nameof(ServerRegistrationDto.ServerAddress));
            DefineMap<ServerRegistration, ServerRegistrationDto>(nameof(ServerRegistration.CreateDate), nameof(ServerRegistrationDto.DateRegistered));
            DefineMap<ServerRegistration, ServerRegistrationDto>(nameof(ServerRegistration.UpdateDate), nameof(ServerRegistrationDto.DateAccessed));
            DefineMap<ServerRegistration, ServerRegistrationDto>(nameof(ServerRegistration.ServerIdentity), nameof(ServerRegistrationDto.ServerIdentity));
        }
    }
}
