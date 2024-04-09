using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

public interface IPermissionMapper
{
    string Context { get; }
    IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto);
}
