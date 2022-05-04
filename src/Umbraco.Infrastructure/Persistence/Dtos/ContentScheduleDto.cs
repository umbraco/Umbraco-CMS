using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
internal class ContentScheduleDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentSchedule;

    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = false)]
    public Guid Id { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    [NullSetting(NullSetting = NullSettings.Null)] // can be invariant
    public int? LanguageId { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("action")]
    public string? Action { get; set; }
}
