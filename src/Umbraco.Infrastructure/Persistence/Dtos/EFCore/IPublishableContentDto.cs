namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
///     Defines the contract for an EF Core publishable content DTO — the EF Core counterpart of the NPoco
///     <c>IPublishableContentDto&lt;TVersionDto&gt;</c>, reduced to the columns the publishable repository base
///     reads and writes generically. The rows persisted alongside the entity row are passed separately, never
///     aggregated here.
/// </summary>
internal interface IPublishableContentDto
{
    /// <summary>Gets or sets the node identifier (primary key shared across content tables).</summary>
    int NodeId { get; set; }

    /// <summary>Gets or sets a value indicating whether the content is currently published.</summary>
    bool Published { get; set; }

    /// <summary>Gets or sets a value indicating whether the content has been edited since its last publish.</summary>
    bool Edited { get; set; }
}
