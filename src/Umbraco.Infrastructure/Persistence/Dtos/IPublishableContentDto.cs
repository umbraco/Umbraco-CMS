using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Defines the contract for a publishable content DTO, combining node identity with publish state and version references.
/// </summary>
/// <typeparam name="TContentVersionDto">The type of the content version DTO.</typeparam>
internal interface IPublishableContentDto<TContentVersionDto> : INodeDto
    where TContentVersionDto : class, IContentVersionDto
{
    /// <summary>
    /// Contains the column name constants for <see cref="IPublishableContentDto{TContentVersionDto}"/>.
    /// </summary>
    internal static class Columns
    {
        /// <summary>
        /// The column name for the node identifier.
        /// </summary>
        internal const string NodeId = INodeDto.Columns.NodeId;

        /// <summary>
        /// The column name for the published flag.
        /// </summary>
        internal const string Published = IContentVersionDto.Columns.Published;

        /// <summary>
        /// The column name for the edited flag.
        /// </summary>
        internal const string Edited = ICultureVariationDto.Columns.Edited;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content is published.
    /// </summary>
    [Column(Columns.Published)]
    bool Published { get; set;  }

    /// <summary>
    /// Gets or sets a value indicating whether the content has been edited since last publish.
    /// </summary>
    [Column(Columns.Edited)]
    bool Edited { get; set; }

    /// <summary>
    /// Gets the <see cref="Dtos.ContentDto"/> containing the core content data.
    /// </summary>
    [ResultColumn]
    ContentDto ContentDto { get; }

    /// <summary>
    /// Gets the current content version DTO.
    /// </summary>
    [ResultColumn]
    TContentVersionDto ContentVersionDto { get; }

    /// <summary>
    /// Gets the published content version DTO, or <c>null</c> if no published version exists.
    /// </summary>
    [ResultColumn]
    TContentVersionDto? PublishedVersionDto { get; }
}
