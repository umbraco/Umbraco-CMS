using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class TagDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Tag;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string LanguageIdColumnName = "languageId";
    private const string GroupColumnName = "group";
    private const string TextColumnName = "tag";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(GroupColumnName)]
    [Length(100)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_languageId_group", ForColumns = $"{LanguageIdColumnName},{GroupColumnName}", IncludeColumns = $"{PrimaryKeyColumnName},{TextColumnName}")]
    public string Group { get; set; } = null!;

    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LanguageId { get; set; }

    [Column(TextColumnName)]
    [Length(200)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{GroupColumnName},{TextColumnName},{LanguageIdColumnName}", Name = "IX_cmsTags")]
    public string Text { get; set; } = null!;

    // [Column("key")]
    // [Length(301)] // de-normalized "{group}/{tag}"
    // public string Key { get; set; }

    // queries result column
    [ResultColumn("NodeCount")]
    public int NodeCount { get; set; }
}
