using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class DocumentCultureVariationDto : ICultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentCultureVariation;

    private const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the document culture variation.
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
    /// Gets or sets the language identifier associated with the document culture variation.
    /// </summary>
    [Column(ICultureVariationDto.Columns.LanguageId)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the culture identifier (e.g., "en-US") associated with this document variation.
    /// This property is not persisted in the database.
    /// </summary>
    /// <remarks>this is convenient to carry the culture around, but has no db counterpart</remarks>
    [Ignore]
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this culture variation of the document has been edited.
    /// </summary>
    /// <remarks>authority on whether a culture has been edited</remarks>
    [Column(ICultureVariationDto.Columns.Edited)]
    public bool Edited { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there is a current content version culture variation for the language.
    /// </summary>
    /// <remarks>
    /// de-normalized for perfs
    /// (means there is a current content version culture variation for the language)
    /// </remarks>
    [Column(ICultureVariationDto.Columns.Available)]
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a published content version exists for this culture and language.
    /// </summary>
    /// <remarks>
    /// de-normalized for perfs
    /// (means there is a published content version culture variation for the language)
    /// </remarks>
    [Column(ICultureVariationDto.Columns.Published)]
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets the denormalized name for the document's culture variation.
    /// This value is typically copied from the current content version's culture variation for the specified language, or from the published version if unavailable.
    /// </summary>
    /// <remarks>
    /// de-normalized for perfs
    /// (when available, copies name from current content version culture variation for the language)
    /// (otherwise, it's the published one, 'cos we need to have one)
    /// </remarks>
    [NullSetting(NullSetting = NullSettings.Null)]
    [Column(ICultureVariationDto.Columns.Name)]
    public string? Name { get; set; }
}
