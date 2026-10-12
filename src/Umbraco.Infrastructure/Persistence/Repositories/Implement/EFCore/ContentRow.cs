using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The final shape a publishable content read query materializes: one row per entity version, carrying
///     the DTOs entity assembly needs plus the optional published version and the parent node's key.
/// </summary>
/// <remarks>
///     A positional record is fine here because nothing orders or filters after the projection; see
///     <see cref="ContentJoinRow{TEntityDto,TContentVersionDto}" /> for the shape that must stay composable.
/// </remarks>
/// <typeparam name="TEntityDto">The EF Core DTO type for the primary entity table.</typeparam>
/// <typeparam name="TContentVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
internal sealed record ContentRow<TEntityDto, TContentVersionDto>(
    NodeDto Node,
    TEntityDto Entity,
    ContentDto Content,
    ContentVersionDto ContentVersion,
    TContentVersionDto EntityVersion,
    ContentVersionDto? PublishedContentVersion,
    TContentVersionDto? PublishedEntityVersion,
    Guid? ParentUniqueId)
    where TEntityDto : class, IPublishableContentDto<TContentVersionDto>
    where TContentVersionDto : class, IContentVersionDto;
