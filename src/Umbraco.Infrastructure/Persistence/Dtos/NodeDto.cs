using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id")]
[ExplicitColumns]
public class NodeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Node;
    public const int NodeIdSeed = 1060;
    private int? _userId;

    [Column("id")]
    [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
    public int NodeId { get; set; }

    [Column("uniqueId")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_UniqueId", IncludeColumns = "parentId,level,path,sortOrder,trashed,nodeUser,text,createDate")]
    [Constraint(Default = SystemMethods.NewGuid)]
    public Guid UniqueId { get; set; }

    [Column("parentId")]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_ParentId")]
    public int ParentId { get; set; }

    // NOTE: This index is primarily for the nucache data lookup, see https://github.com/umbraco/Umbraco-CMS/pull/8365#issuecomment-673404177
    [Column("level")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Level", ForColumns = "level,parentId,sortOrder,nodeObjectType,trashed", IncludeColumns = "nodeUser,path,uniqueId,createDate")]
    public short Level { get; set; }

    [Column("path")]
    [Length(150)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Path")]
    public string Path { get; set; } = null!;

    [Column("sortOrder")]
    public int SortOrder { get; set; }

    [Column("trashed")]
    [Constraint(Default = "0")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Trashed")]
    public bool Trashed { get; set; }

    [Column("nodeUser")] // TODO: db rename to 'createUserId'
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; } // return null if zero

    [Column("text")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Text { get; set; }

    [Column("nodeObjectType")] // TODO: db rename to 'objectType'
    [NullSetting(NullSetting = NullSettings.Null)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_ObjectType", ForColumns = "nodeObjectType,trashed", IncludeColumns = "uniqueId,parentId,level,path,sortOrder,nodeUser,text,createDate")]
    public Guid? NodeObjectType { get; set; }

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; }
}
