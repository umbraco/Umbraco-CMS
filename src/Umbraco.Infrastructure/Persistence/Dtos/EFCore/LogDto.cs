using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(LogDtoConfiguration))]
public class LogDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Log;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    // Public constants to bind properties between configurations and customizers.
    public const string IdColumnName = PrimaryKeyColumnName;
    public const string UserIdColumnName = "userId";
    public const string NodeIdColumnName = "NodeId";
    public const string EntityTypeColumnName = "entityType";
    public const string DatestampColumnName = "Datestamp";
    public const string HeaderColumnName = "logHeader";
    public const string CommentColumnName = "logComment";
    public const string ParametersColumnName = "parameters";

    private int? _userId;

    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user associated with the log entry, or <c>null</c> if the entry is not associated with a user.
    /// </summary>
    /// <remarks>return null if zero</remarks>
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; }

    /// <summary>
    /// Gets or sets the node identifier.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// This is the entity type associated with the log.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the log entry was created.
    /// </summary>
    public DateTime Datestamp { get; set; }

    /// <summary>
    /// Gets or sets the header, which is a short description or category for the log entry.
    /// </summary>
    public string Header { get; set; } = null!;

    /// <summary>
    /// Gets or sets the comment associated with the log entry.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Used to store additional data parameters for the log.
    /// </summary>
    public string? Parameters { get; set; }
}
