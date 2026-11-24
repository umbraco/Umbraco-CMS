using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The final shape a content read query materializes: one row per entity version, carrying the DTOs entity
///     assembly needs plus the parent node's key. Entity kinds with more tables extend it.
/// </summary>
/// <remarks>
///     A positional record is fine here because nothing orders or filters after the projection; see
///     <see cref="ContentJoinRow{TVersionDto}" /> for the shape that must stay composable.
/// </remarks>
/// <typeparam name="TVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
internal record ContentRow<TVersionDto>(
    NodeDto Node,
    ContentDto Content,
    ContentVersionDto ContentVersion,
    TVersionDto EntityVersion,
    Guid? ParentUniqueId)
    where TVersionDto : class, IEntityVersionDto;
