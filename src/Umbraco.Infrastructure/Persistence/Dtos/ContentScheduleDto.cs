using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ContentScheduleDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentSchedule;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    public Guid Id { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    [NullSetting(NullSetting = NullSettings.Null)] // can be invariant
    public int? LanguageId { get; set; }

    // NOTE: this date is explicitly stored and treated as UTC despite the lack of "Utc" postfix.
    [Column("date")]
    public DateTime Date { get; set; }

    [Column("action")]
    public string? Action { get; set; }
}
