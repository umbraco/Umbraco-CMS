using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class ContentVersionCultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCultureVariation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string LanguageIdColumnName = "languageId";
    private const string VersionIdColumnName = "versionId";
    private const string UpdateUserIdColumnName = "availableUserId";
    private const string UpdateDateColumnName = "date";
    private const string NameColumnName = "name";

    private int? _updateUserId;

    /// <summary>
    /// Gets or sets the unique identifier for the content version culture variation.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated content version.
    /// </summary>
    [Column(VersionIdColumnName)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = $"{VersionIdColumnName},{LanguageIdColumnName}", IncludeColumns = $"{PrimaryKeyColumnName},{NameColumnName},{UpdateDateColumnName},{UpdateUserIdColumnName}")]
    public int VersionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the language associated with this content version culture variation.
    /// </summary>
    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the culture identifier for this content version variation. This property is for convenience only and is not mapped to the database.
    /// </summary>
    /// <remarks>this is convenient to carry the culture around, but has no db counterpart</remarks>
    [Ignore]
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the name associated with this culture variation of the content version.
    /// </summary>
    [Column(NameColumnName)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the content version culture variation was last updated.
    /// </summary>
    /// <remarks>TODO: db rename to 'updateDate'</remarks>
    [Column(UpdateDateColumnName)]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this content version culture variation.
    /// </summary>
    /// <remarks>TODO: db rename to 'updateUserId'</remarks>
    [Column(UpdateUserIdColumnName)]
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UpdateUserId
    {
        get => _updateUserId == 0 ? null : _updateUserId;
        set => _updateUserId = value;
    } // return null if zero
}
