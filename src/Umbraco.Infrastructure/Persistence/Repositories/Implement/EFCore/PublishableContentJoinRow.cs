using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The join shape of a publishable content repository: <see cref="ContentJoinRow{TVersionDto}" /> plus the
///     entity row and the LEFT JOINed published version.
/// </summary>
/// <typeparam name="TEntityDto">The EF Core DTO type for the primary entity table.</typeparam>
/// <typeparam name="TVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
internal sealed class PublishableContentJoinRow<TEntityDto, TVersionDto> : ContentJoinRow<TVersionDto>
    where TEntityDto : class, IPublishableContentDto
    where TVersionDto : class, IPublishableVersionDto
{
    public TEntityDto Entity { get; set; } = null!;

    public ContentVersionDto? PublishedContentVersion { get; set; }

    public TVersionDto? PublishedEntityVersion { get; set; }
}
