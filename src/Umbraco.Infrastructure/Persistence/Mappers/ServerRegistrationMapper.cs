using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(ServerRegistration))]
[MapperFor(typeof(IServerRegistration))]
internal sealed class ServerRegistrationMapper : BaseMapper
{
    public ServerRegistrationMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<ServerRegistration, ServerRegistrationDto>(
            nameof(ServerRegistration.Id),
            nameof(ServerRegistrationDto.Id));
        DefineMap<ServerRegistration, ServerRegistrationDto>(
            nameof(ServerRegistration.IsActive),
            nameof(ServerRegistrationDto.IsActive));
        DefineMap<ServerRegistration, ServerRegistrationDto>(
            nameof(ServerRegistration.IsSchedulingPublisher),
            nameof(ServerRegistrationDto.IsSchedulingPublisher));
        DefineMap<ServerRegistration, ServerRegistrationDto>(
            nameof(ServerRegistration.ServerAddress),
            nameof(ServerRegistrationDto.ServerAddress));
        DefineMap<ServerRegistration, ServerRegistrationDto>(
            nameof(ServerRegistration.CreateDate),
            nameof(ServerRegistrationDto.DateRegistered));
        DefineMap<ServerRegistration, ServerRegistrationDto>(
            nameof(ServerRegistration.UpdateDate),
            nameof(ServerRegistrationDto.DateAccessed));
        DefineMap<ServerRegistration, ServerRegistrationDto>(
            nameof(ServerRegistration.ServerIdentity),
            nameof(ServerRegistrationDto.ServerIdentity));
    }
}
