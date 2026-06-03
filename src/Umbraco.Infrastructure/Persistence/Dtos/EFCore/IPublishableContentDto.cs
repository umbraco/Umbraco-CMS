namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
///     Defines the contract for an EF Core publishable content DTO — the EF Core counterpart of the NPoco
///     <c>IPublishableContentDto&lt;TVersionDto&gt;</c>. The implementing DTO aggregates its nav properties
///     (populated by the repository after a join query, mirroring what NPoco <c>[Reference]</c> attributes do).
/// </summary>
/// <typeparam name="TVersionDto">
///     The type-specific version DTO (e.g. <see cref="DocumentVersionDto" />).
/// </typeparam>
internal interface IPublishableContentDto<TVersionDto>
    where TVersionDto : class, IContentVersionDto
{
    /// <summary>Gets the node identifier (primary key shared across content tables).</summary>
    int NodeId { get; }

    /// <summary>Gets a value indicating whether the content is currently published.</summary>
    bool Published { get; }

    /// <summary>Gets a value indicating whether the content has been edited since its last publish.</summary>
    bool Edited { get; }

    /// <summary>
    ///     Gets the content row, which carries <see cref="ContentDto.NodeDto" /> and
    ///     <see cref="ContentDto.ContentTypeId" />.
    /// </summary>
    ContentDto ContentDto { get; }

    /// <summary>Gets the current (draft) version row.</summary>
    TVersionDto CurrentVersion { get; }

    /// <summary>Gets the published version row, or <c>null</c> if the content has never been published.</summary>
    TVersionDto? PublishedVersion { get; }
}
