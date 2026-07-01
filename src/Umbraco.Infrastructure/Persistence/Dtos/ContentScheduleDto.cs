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

    /// <summary>
    /// Gets or sets the unique identifier of the content schedule.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content node associated with this schedule.
    /// </summary>
    [Column("nodeId")]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the language associated with the content schedule.
    /// A null value indicates the schedule is invariant (not language-specific).
    /// </summary>
    /// <remarks>can be invariant</remarks>
    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled date and time for the content action.
    /// This value is explicitly stored and treated as UTC, even though the property name does not include a 'Utc' suffix.
    /// </summary>
    /// <remarks>NOTE: this date is explicitly stored and treated as UTC despite the lack of "Utc" postfix.</remarks>
    [Column("date")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the action to be performed as part of the content schedule, such as publishing or unpublishing content.
    /// </summary>
    [Column("action")]
    public string? Action { get; set; }
}
