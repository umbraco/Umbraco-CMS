using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class AuditItemFactory
{
    public static IEnumerable<IAuditItem> BuildEntities(IEnumerable<LogDto> dtos) =>
        dtos.Select(BuildEntity).ToList();

    public static IAuditItem BuildEntity(LogDto dto)
        => new AuditItem(
            dto.NodeId,
            Enum<AuditType>.ParseOrNull(dto.Header) ?? AuditType.Custom,
            dto.UserId ?? Constants.Security.UnknownUserId,
            dto.EntityType,
            dto.Comment,
            dto.Parameters,
            dto.Datestamp.EnsureUtc());

    public static LogDto BuildDto(IAuditItem entity) =>
        new LogDto
        {
            Comment = entity.Comment,
            Datestamp = DateTime.UtcNow,
            Header = entity.AuditType.ToString(),
            NodeId = entity.Id,
            UserId = entity.UserId,
            EntityType = entity.EntityType,
            Parameters = entity.Parameters,
        };
}
