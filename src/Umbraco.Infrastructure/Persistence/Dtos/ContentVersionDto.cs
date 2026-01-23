using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class ContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    internal const string ReferenceColumnName = "NodeId"; // should be ContentTypeDto.NodeIdColumnName, but for database compatibility we keep it like this

    private const string UserIdColumnName = "userId";
    private const string VersionDateColumnName = "versionDate";
    private const string CurrentColumnName = "current";
    private const string TextColumnName = "text";
    private const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    private const string PreventCleanupColumnName = "preventCleanup";

    private int? _userId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(ContentDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = $"{NodeIdColumnName},{CurrentColumnName}", IncludeColumns = $"{PrimaryKeyColumnName},{VersionDateColumnName},{TextColumnName},{UserIdColumnName},{PreventCleanupColumnName}")]
    public int NodeId { get; set; }

    [Column(VersionDateColumnName)] // TODO: db rename to 'updateDate'
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime VersionDate { get; set; }

    [Column(UserIdColumnName)] // TODO: db rename to 'updateUserId'
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; } // return null if zero

    [Column(CurrentColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Current", IncludeColumns = NodeIdColumnName)]
    public bool Current { get; set; }

    // about current:
    // there is nothing in the DB that guarantees that there will be one, and exactly one, current version per content item.
    // that would require circular FKs that are impossible (well, it is possible to create them, but not to insert).
    // we could use a content.currentVersionId FK that would need to be nullable, or (better?) an additional table
    // linking a content itemt to its current version (nodeId, versionId) - that would guarantee uniqueness BUT it would
    // not guarantee existence - so, really... we are trusting our code to manage 'current' correctly.
    [Column(TextColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Text { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = ReferenceColumnName, ReferenceMemberName = ContentDto.ReferenceMemberName)]
    public ContentDto? ContentDto { get; set; }

    [Column(PreventCleanupColumnName)]
    [Constraint(Default = "0")]
    public bool PreventCleanup { get; set; }
}
