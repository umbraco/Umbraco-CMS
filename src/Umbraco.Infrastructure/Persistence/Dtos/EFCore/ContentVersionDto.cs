using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentVersionDtoConfiguration))]
public class ContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string KeyColumnName = "key";
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string VersionDateColumnName = "versionDate";
    public const string UserIdColumnName = "userId";
    public const string CurrentColumnName = "current";
    public const string TextColumnName = "text";
    public const string PreventCleanupColumnName = "preventCleanup";

    private int? _userId;

    /// <summary>
    /// Gets or sets the unique identifier for the content version.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the globally unique key for the content version.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the node for this content version.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the content version was last updated.
    /// </summary>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified or updated this content version.
    /// A value of <c>null</c> indicates that no user is associated with the update.
    /// </summary>
    /// <remarks>Returns null if zero.</remarks>
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the current version of the content.
    /// </summary>
    public bool Current { get; set; }

    /// <summary>
    /// Gets or sets the textual representation associated with this content version.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this content version is protected from cleanup operations.
    /// </summary>
    public bool PreventCleanup { get; set; }
}
