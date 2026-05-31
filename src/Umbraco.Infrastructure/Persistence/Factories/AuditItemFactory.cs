using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class AuditItemFactory
{
    /// <summary>
    /// Builds a collection of <see cref="IAuditItem"/> entities from the given collection of <see cref="LogDto"/> objects.
    /// </summary>
    /// <param name="dtos">The collection of <see cref="LogDto"/> objects to convert.</param>
    /// <returns>An enumerable collection of <see cref="IAuditItem"/> entities.</returns>
    public static IEnumerable<IAuditItem> BuildEntities(IEnumerable<LogDto> dtos) =>
        dtos.Select(BuildEntity).ToList();

    /// <summary>
    /// Builds an <see cref="IAuditItem"/> entity from the given <see cref="LogDto"/>.
    /// </summary>
    /// <param name="dto">The data transfer object containing log information.</param>
    /// <returns>An <see cref="IAuditItem"/> representing the audit item.</returns>
    public static IAuditItem BuildEntity(LogDto dto)
        => new AuditItem(
            dto.NodeId,
            Enum<AuditType>.ParseOrNull(dto.Header) ?? AuditType.Custom,
            dto.UserId ?? Constants.Security.UnknownUserId,
            dto.EntityType,
            dto.Comment,
            dto.Parameters,
            dto.Datestamp.EnsureUtc());

    /// <summary>
    /// Builds a <see cref="Umbraco.Cms.Core.Models.Membership.LogDto"/> from the given <see cref="Umbraco.Cms.Core.Models.IAuditItem"/> entity.
    /// </summary>
    /// <param name="entity">The audit item entity to convert.</param>
    /// <returns>A <see cref="Umbraco.Cms.Core.Models.Membership.LogDto"/> representing the audit item.</returns>
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
