using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DocumentCultureVariationDtoConfiguration))]
public class DocumentCultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentCultureVariation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string LanguageIdColumnName = "languageId";
    public const string EditedColumnName = "edited";
    public const string AvailableColumnName = "available";
    public const string PublishedColumnName = "published";
    public const string NameColumnName = "name";

    /// <summary>
    /// Gets or sets the unique identifier for the document culture variation.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content node associated with this culture variation.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the language identifier associated with the document culture variation.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this culture variation of the document has been edited.
    /// </summary>
    public bool Edited { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there is a current content version culture variation for the language.
    /// </summary>
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a published content version exists for this culture and language.
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets the denormalized name for the document's culture variation.
    /// </summary>
    public string? Name { get; set; }
}
