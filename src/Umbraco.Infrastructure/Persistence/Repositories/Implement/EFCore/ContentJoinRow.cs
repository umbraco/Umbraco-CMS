using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The join shape every read query of a publishable content repository is built on: the node, entity and
///     content rows, the selected version in both its shared and entity-specific form, and the LEFT JOINed
///     content type, published version and parent node.
/// </summary>
/// <remarks>
///     Built via object-initializer syntax only, never a positional constructor call: EF Core cannot translate
///     member access into a type built from a constructor call when a subsequent OrderBy targets it, but
///     member-init projections translate and order correctly.
/// </remarks>
/// <typeparam name="TEntityDto">The EF Core DTO type for the primary entity table.</typeparam>
/// <typeparam name="TContentVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
internal sealed class ContentJoinRow<TEntityDto, TContentVersionDto>
    where TEntityDto : class, IPublishableContentDto<TContentVersionDto>
    where TContentVersionDto : class, IContentVersionDto
{
    public NodeDto Node { get; set; } = null!;

    public TEntityDto Entity { get; set; } = null!;

    public ContentDto Content { get; set; } = null!;

    public ContentVersionDto ContentVersion { get; set; } = null!;

    public TContentVersionDto EntityVersion { get; set; } = null!;

    // No FK is declared from Content.ContentTypeId to ContentTypeDto in the EF model, so nothing
    // guarantees every row resolves one - LEFT JOINed and nullable rather than assumed present.
    public ContentTypeDto? ContentType { get; set; }

    public ContentVersionDto? PublishedContentVersion { get; set; }

    public TContentVersionDto? PublishedEntityVersion { get; set; }

    public NodeDto? ParentNode { get; set; }
}
