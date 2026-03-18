using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) that encapsulates information about a specific version of content within Umbraco CMS.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class ContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string UserIdColumnName = "userId";
    private const string VersionDateColumnName = "versionDate";
    private const string CurrentColumnName = "current";
    private const string TextColumnName = "text";
    private const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    private const string PreventCleanupColumnName = "preventCleanup";

    private int? _userId;

    /// <summary>
    /// Gets or sets the unique identifier for the content version.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the node for this content version.
    /// </summary>
    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(ContentDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = $"{NodeIdColumnName},{CurrentColumnName}", IncludeColumns = $"{PrimaryKeyColumnName},{VersionDateColumnName},{TextColumnName},{UserIdColumnName},{PreventCleanupColumnName}")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the content version was last updated.
    /// </summary>
    /// <remarks>TODO: db rename to 'updateDate'</remarks>
    [Column(VersionDateColumnName)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified or updated this content version.
    /// A value of <c>null</c> indicates that no user is associated with the update.
    /// </summary>
    /// <remarks>
    /// Returns null if zero.
    /// TODO: db rename to 'updateUserId'.
    /// </remarks>
    [Column(UserIdColumnName)]
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the current version of the content.
    /// </summary>
    [Column(CurrentColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Current", IncludeColumns = NodeIdColumnName)]
    public bool Current { get; set; }

    /// <summary>
    /// Gets or sets the textual representation associated with this content version.
    /// </summary>
    /// <remarks>
    /// about current:
    /// there is nothing in the DB that guarantees that there will be one, and exactly one, current version per content item.
    /// that would require circular FKs that are impossible (well, it is possible to create them, but not to insert).
    /// we could use a content.currentVersionId FK that would need to be nullable, or (better?) an additional table
    /// linking a content itemt to its current version (nodeId, versionId) - that would guarantee uniqueness BUT it would
    /// not guarantee existence - so, really... we are trusting our code to manage 'current' correctly.
    /// </remarks>
    [Column(TextColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentDto"/> associated with this content version.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(NodeId), ReferenceMemberName = nameof(ContentDto.NodeId))]
    public ContentDto? ContentDto { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this content version is protected from cleanup operations.
    /// </summary>
    [Column(PreventCleanupColumnName)]
    [Constraint(Default = "0")]
    public bool PreventCleanup { get; set; }
}
