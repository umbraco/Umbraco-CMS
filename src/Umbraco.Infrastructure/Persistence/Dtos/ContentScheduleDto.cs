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
    /// The name of the index on the content node identifier, used when schedules are read or deleted for a node.
    /// </summary>
    internal const string NodeIdIndexName = "IX_" + TableName + "_" + NodeIdColumnName;

    /// <summary>
    /// The name of the index on the action and date, used when scheduled publishing polls for due schedules.
    /// </summary>
    internal const string ActionDateIndexName = "IX_" + TableName + "_" + ActionColumnName + "_" + DateColumnName;

    private const string NodeIdColumnName = "nodeId";
    private const string LanguageIdColumnName = "languageId";
    private const string DateColumnName = "date";
    private const string ActionColumnName = "action";

    /// <summary>
    /// Gets or sets the unique identifier of the content schedule.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content node associated with this schedule.
    /// </summary>
    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(ContentDto))]
    [Index(IndexTypes.NonClustered, Name = NodeIdIndexName)]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the language associated with the content schedule.
    /// A null value indicates the schedule is invariant (not language-specific).
    /// </summary>
    /// <remarks>can be invariant</remarks>
    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled date and time for the content action.
    /// This value is explicitly stored and treated as UTC, even though the property name does not include a 'Utc' suffix.
    /// </summary>
    /// <remarks>NOTE: this date is explicitly stored and treated as UTC despite the lack of "Utc" postfix.</remarks>
    [Column(DateColumnName)]
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the action to be performed as part of the content schedule, such as publishing or unpublishing content.
    /// </summary>
    [Column(ActionColumnName)]
    [Index(IndexTypes.NonClustered, Name = ActionDateIndexName, ForColumns = $"{ActionColumnName},{DateColumnName}", IncludeColumns = $"{NodeIdColumnName},{LanguageIdColumnName}")]
    public string? Action { get; set; }
}
