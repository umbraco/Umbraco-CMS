using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The join shape every read query of a content repository is built on: the node and content rows, the
///     selected version in both its shared and entity-specific form, and the LEFT JOINed content type and parent
///     node. Entity kinds with more tables extend it.
/// </summary>
/// <remarks>
///     Built via object-initializer syntax only, never a positional constructor call: EF Core cannot translate
///     member access into a type built from a constructor call when a subsequent OrderBy targets it, but
///     member-init projections translate and order correctly.
/// </remarks>
/// <typeparam name="TVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
internal class ContentJoinRow<TVersionDto>
    where TVersionDto : class, IEntityVersionDto
{
    public NodeDto Node { get; set; } = null!;

    public ContentDto Content { get; set; } = null!;

    public ContentVersionDto ContentVersion { get; set; } = null!;

    public TVersionDto EntityVersion { get; set; } = null!;

    // No FK is declared from Content.ContentTypeId to ContentTypeDto in the EF model, so nothing
    // guarantees every row resolves one - LEFT JOINed and nullable rather than assumed present.
    public ContentTypeDto? ContentType { get; set; }

    public NodeDto? ParentNode { get; set; }
}
