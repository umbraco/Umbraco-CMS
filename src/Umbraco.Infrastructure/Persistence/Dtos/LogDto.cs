using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class LogDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Log;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string UserIdColumnName = "userId";
    private const string NodeIdColumnName = "NodeId";
    private const string DatestampColumnName = "Datestamp";
    private const string HeaderColumnName = "logHeader";
    private int? _userId;

    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user associated with the log entry, or <c>null</c> if the entry is not associated with a user.
    /// </summary>
    /// <remarks>return null if zero</remarks>
    [Column(UserIdColumnName)]
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; }

    /// <summary>
    /// Gets or sets the node identifier.
    /// </summary>
    [Column(NodeIdColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoLog")]
    public int NodeId { get; set; }

    /// <summary>
    ///     This is the entity type associated with the log
    /// </summary>
    [Column("entityType")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the log entry was created.
    /// </summary>
    [Column(DatestampColumnName)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_datestamp", ForColumns = $"{DatestampColumnName},{UserIdColumnName},{NodeIdColumnName}")]
    public DateTime Datestamp { get; set; }

    /// <summary>
    /// Gets or sets the header, which is a short description or category for the log entry.
    /// </summary>
    [Column(HeaderColumnName)]
    [Length(50)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_datestamp_logheader", ForColumns = $"{DatestampColumnName},{HeaderColumnName}")]
    public string Header { get; set; } = null!;

    /// <summary>
    /// Gets or sets the comment associated with the log entry.
    /// </summary>
    [Column("logComment")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(4000)]
    public string? Comment { get; set; }

    /// <summary>
    ///     Used to store additional data parameters for the log
    /// </summary>
    [Column("parameters")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(4000)]
    public string? Parameters { get; set; }
}
