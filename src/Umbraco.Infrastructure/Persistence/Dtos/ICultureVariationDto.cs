using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Defines the contract for a culture variation DTO, representing culture-specific data for a content node.
/// </summary>
internal interface ICultureVariationDto : INodeDto
{
    /// <summary>
    /// Contains the column name constants for <see cref="ICultureVariationDto"/>.
    /// </summary>
    internal static class Columns
    {
        /// <summary>
        /// The column name for the node identifier.
        /// </summary>
        internal const string NodeId = INodeDto.Columns.NodeId;

        /// <summary>
        /// The column name for the language identifier.
        /// </summary>
        internal const string LanguageId = "languageId";

        /// <summary>
        /// The column name for the edited flag.
        /// </summary>
        internal const string Edited = "edited";

        /// <summary>
        /// The column name for the display name.
        /// </summary>
        internal const string Name = "name";

        /// <summary>
        /// The column name for the available flag.
        /// </summary>
        internal const string Available = "available";

        /// <summary>
        /// The column name for the published flag.
        /// </summary>
        internal const string Published = IContentVersionDto.Columns.Published;
    }

    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    [Column(Columns.LanguageId)]
    int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this culture variation has been edited.
    /// </summary>
    [Column(Columns.Edited)]
    bool Edited { get; set; }

    /// <summary>
    /// Gets or sets the culture identifier (e.g., "en-US"). This property is not persisted in the database.
    /// </summary>
    [Ignore]
    string? Culture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there is a current content version culture variation for the language.
    /// </summary>
    [Column(Columns.Available)]
    bool Available { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a published content version exists for this culture and language.
    /// </summary>
    /// <remarks>De-normalized for performance.</remarks>
    [Column(Columns.Published)]
    bool Published { get; set; }

    /// <summary>
    /// Gets or sets the display name for this culture variation.
    /// </summary>
    [Column(Columns.Name)]
    string? Name { get; set; }
}
