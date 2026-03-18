using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// TODO ELEMENTS: split this into two interfaces - like "IEntityDto" and "IPublishedDto"?

/// <summary>
/// Defines the contract for a content version DTO with an identifier and published state.
/// </summary>
internal interface IContentVersionDto
{
    /// <summary>
    /// Contains the column name constants for <see cref="IContentVersionDto"/>.
    /// </summary>
    internal static class Columns
    {
        /// <summary>
        /// The column name for the version identifier.
        /// </summary>
        internal const string Id = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

        /// <summary>
        /// The column name for the published flag.
        /// </summary>
        internal const string Published = "published";
    }

    /// <summary>
    /// Gets or sets the version identifier.
    /// </summary>
    [Column(Columns.Id)]
    int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this version is published.
    /// </summary>
    [Column(Columns.Published)]
    bool Published { get; set; }

    /// <summary>
    /// Gets the <see cref="Dtos.ContentVersionDto"/> associated with this version.
    /// </summary>
    [ResultColumn]
    ContentVersionDto ContentVersionDto { get; }
}
