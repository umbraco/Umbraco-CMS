using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class ServerRegistrationFactory
{
    public static ServerRegistration BuildEntity(ServerRegistrationDto dto)
    {
        var model = new ServerRegistration(dto.Id, dto.ServerAddress, dto.ServerIdentity, dto.DateRegistered, dto.DateAccessed, dto.IsActive, dto.IsSchedulingPublisher);

        // reset dirty initial properties (U4-1946)
        model.ResetDirtyProperties(false);
        return model;
    }

    public static ServerRegistrationDto BuildDto(IServerRegistration entity)
    {
        var dto = new ServerRegistrationDto
        {
            ServerAddress = entity.ServerAddress,
            DateRegistered = entity.CreateDate,
            IsActive = entity.IsActive,
            IsSchedulingPublisher = ((ServerRegistration)entity).IsSchedulingPublisher,
            DateAccessed = entity.UpdateDate,
            ServerIdentity = entity.ServerIdentity,
        };

        if (entity.HasIdentity)
        {
            dto.Id = entity.Id;
        }

        return dto;
    }
}
