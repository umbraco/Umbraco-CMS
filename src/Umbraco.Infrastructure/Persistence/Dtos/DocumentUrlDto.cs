
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object that encapsulates URL-related information for a document entity in Umbraco CMS.
/// This DTO is typically used for persisting or transferring document URL data between application layers or for database operations.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class DocumentUrlDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentUrl;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string UniqueIdColumnName = "uniqueId";
    public const string IsDraftColumnName = "isDraft";
    public const string LanguageIdColumnName = "languageId";
    public const string UrlSegmentColumnName = "urlSegment";
    public const string IsPrimaryColumnName = "isPrimary";

    /// <summary>
    /// Gets or sets the unique identifier for the node.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Clustered = false, AutoIncrement = true)]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the document node associated with this URL.
    /// </summary>
    [Index(IndexTypes.UniqueClustered, ForColumns = $"{UniqueIdColumnName}, {LanguageIdColumnName}, {IsDraftColumnName}, {UrlSegmentColumnName}", Name = "IX_" + TableName)]
    [Column(UniqueIdColumnName)]
    [ForeignKey(typeof(NodeDto), Column = NodeDto.KeyColumnName)]
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document URL is a draft.
    /// </summary>
    [Column(IsDraftColumnName)]
    public bool IsDraft { get; set; }

    /// <summary>
    /// Gets or sets the language identifier associated with the document URL.
    /// </summary>
    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the URL segment, which is the part of the document's URL typically used for routing and identifying the document in URLs.
    /// </summary>
    [Column(UrlSegmentColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string UrlSegment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this URL is the primary URL.
    /// </summary>
    [Column(IsPrimaryColumnName)]
    [Constraint(Default = 1)]
    public bool IsPrimary { get; set; }
}
