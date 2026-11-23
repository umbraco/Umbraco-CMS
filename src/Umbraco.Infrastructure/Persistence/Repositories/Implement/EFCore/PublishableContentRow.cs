using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The row a publishable content read query materializes: <see cref="ContentRow{TVersionDto}" /> plus the
///     entity row and the optional published version.
/// </summary>
/// <typeparam name="TEntityDto">The EF Core DTO type for the primary entity table.</typeparam>
/// <typeparam name="TVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
internal sealed record PublishableContentRow<TEntityDto, TVersionDto>(
    NodeDto Node,
    TEntityDto Entity,
    ContentDto Content,
    ContentVersionDto ContentVersion,
    TVersionDto EntityVersion,
    ContentVersionDto? PublishedContentVersion,
    TVersionDto? PublishedEntityVersion,
    Guid? ParentUniqueId)
    : ContentRow<TVersionDto>(Node, Content, ContentVersion, EntityVersion, ParentUniqueId)
    where TEntityDto : class, IPublishableContentDto
    where TVersionDto : class, IContentVersionDto;
