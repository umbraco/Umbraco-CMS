using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Represents a contract for mapping permission entities between the domain model and the database.
/// </summary>
public interface IPermissionMapper
{
    /// <summary>
    /// Gets the context string that identifies the scope or area to which the permission mapper applies.
    /// </summary>
    string Context { get; }

    /// <summary>
    /// Maps a <see cref="UserGroup2GranularPermissionDto"/> to an <see cref="IGranularPermission"/> instance.
    /// </summary>
    /// <param name="dto">The DTO representing a user group's granular permission.</param>
    /// <returns>The corresponding <see cref="IGranularPermission"/> instance.</returns>
    IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto);
}
