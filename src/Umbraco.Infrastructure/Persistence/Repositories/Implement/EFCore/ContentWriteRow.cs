using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The rows a publishable content write persists for one entity: the node, content and shared content version
///     rows, the entity-specific version row and the entity row, all built from the entity before the write starts.
///     The write-side counterpart of <see cref="ContentRow{TEntityDto,TContentVersionDto}" />.
/// </summary>
/// <typeparam name="TEntityDto">The EF Core DTO type for the primary entity table.</typeparam>
/// <typeparam name="TContentVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
internal sealed record ContentWriteRow<TEntityDto, TContentVersionDto>(
    NodeDto Node,
    ContentDto Content,
    ContentVersionDto ContentVersion,
    TContentVersionDto EntityVersion,
    TEntityDto Entity)
    where TEntityDto : class, IPublishableContentDto
    where TContentVersionDto : class, IContentVersionDto;
