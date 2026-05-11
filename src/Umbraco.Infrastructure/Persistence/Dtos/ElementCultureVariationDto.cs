using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object for the culture-specific variation of an element in the database.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class ElementCultureVariationDto : ICultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ElementCultureVariation;

    private const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the element culture variation.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content node associated with this culture variation.
    /// </summary>
    [Column(ICultureVariationDto.Columns.NodeId)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = $"{INodeDto.Columns.NodeId},{ICultureVariationDto.Columns.LanguageId}")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the language identifier associated with the element culture variation.
    /// </summary>
    [Column(ICultureVariationDto.Columns.LanguageId)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the culture identifier (e.g., "en-US") associated with this element variation.
    /// This property is not persisted in the database.
    /// </summary>
    /// <remarks>This is convenient to carry the culture around, but has no database counterpart.</remarks>
    [Ignore]
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this culture variation of the element has been edited.
    /// </summary>
    [Column(ICultureVariationDto.Columns.Edited)]
    public bool Edited { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there is a current content version culture variation for the language.
    /// </summary>
    /// <remarks>De-normalized for performance.</remarks>
    [Column(ICultureVariationDto.Columns.Available)]
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a published content version exists for this culture and language.
    /// </summary>
    /// <remarks>De-normalized for performance.</remarks>
    [Column(ICultureVariationDto.Columns.Published)]
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets the denormalized name for the element's culture variation.
    /// This value is typically copied from the current content version's culture variation for the specified language, or from the published version if unavailable.
    /// </summary>
    /// <remarks>
    /// De-normalized for performance.
    /// When available, copies name from current content version culture variation for the language;
    /// otherwise, it's the published one, because we need to have one.
    /// </remarks>
    [Column(ICultureVariationDto.Columns.Name)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }
}
