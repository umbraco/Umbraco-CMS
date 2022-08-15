using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id")]
[ExplicitColumns]
public class ContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersion;
    private int? _userId;

    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(ContentDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = "nodeId,current", IncludeColumns = "id,versionDate,text,userId")]
    public int NodeId { get; set; }

    [Column("versionDate")] // TODO: db rename to 'updateDate'
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime VersionDate { get; set; }

    [Column("userId")] // TODO: db rename to 'updateUserId'
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; } // return null if zero

    [Column("current")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Current", IncludeColumns = "nodeId")]
    public bool Current { get; set; }

    // about current:
    // there is nothing in the DB that guarantees that there will be one, and exactly one, current version per content item.
    // that would require circular FKs that are impossible (well, it is possible to create them, but not to insert).
    // we could use a content.currentVersionId FK that would need to be nullable, or (better?) an additional table
    // linking a content itemt to its current version (nodeId, versionId) - that would guarantee uniqueness BUT it would
    // not guarantee existence - so, really... we are trusting our code to manage 'current' correctly.
    [Column("text")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Text { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = "NodeId", ReferenceMemberName = "NodeId")]
    public ContentDto? ContentDto { get; set; }

    [Column("preventCleanup")]
    [Constraint(Default = "0")]
    public bool PreventCleanup { get; set; }
}
