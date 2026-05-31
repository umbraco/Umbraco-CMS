using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class ServerRegistrationFactory
{
    /// <summary>
    /// Builds a <see cref="ServerRegistration"/> entity from the given <see cref="ServerRegistrationDto"/>.
    /// </summary>
    /// <param name="dto">The data transfer object containing server registration data.</param>
    /// <returns>A <see cref="ServerRegistration"/> entity constructed from the DTO.</returns>
    public static ServerRegistration BuildEntity(ServerRegistrationDto dto)
    {
        var model = new ServerRegistration(dto.Id, dto.ServerAddress, dto.ServerIdentity, dto.DateRegistered.EnsureUtc(), dto.DateAccessed.EnsureUtc(), dto.IsActive, dto.IsSchedulingPublisher);

        // reset dirty initial properties (U4-1946)
        model.ResetDirtyProperties(false);
        return model;
    }

    /// <summary>
    /// Builds a <see cref="ServerRegistrationDto"/> from the given <see cref="IServerRegistration"/> entity.
    /// </summary>
    /// <param name="entity">The server registration entity to convert.</param>
    /// <returns>A <see cref="ServerRegistrationDto"/> representing the provided entity.</returns>
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
