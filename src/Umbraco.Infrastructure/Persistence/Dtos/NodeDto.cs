using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class NodeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Node;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const int NodeIdSeed = 1060;

    // Public constants to bind properties between DTOs

    /// <summary>
    /// This actually represents NodeId but kept as Id for backward compatibility.
    /// </summary>
    public const string IdColumnName = PrimaryKeyColumnName;

    public const string KeyColumnName = Constants.DatabaseSchema.Columns.UniqueIdName;
    public const string ParentIdColumnName = "parentId";
    public const string SortOrderColumnName = "sortOrder";
    public const string TrashedColumnName = "trashed";
    public const string NodeObjectTypeColumnName = "nodeObjectType";
    public const string TextColumnName = "text";
    public const string PathColumnName = "path";
    public const string LevelColumnName = "level";
    public const string UserIdColumnName = "nodeUser";
    public const string CreateDateColumnName = "createDate";
    public const string UniqueIdColumnNameTypo = "uniqueID";


    private int? _userId;

    [Column(IdColumnName)]
    [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
    public int NodeId { get; set; }

    [Column(KeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_UniqueId", IncludeColumns = $"{ParentIdColumnName},{LevelColumnName},{PathColumnName},{SortOrderColumnName},{TrashedColumnName},{UserIdColumnName},{TextColumnName},{CreateDateColumnName}")]
    [Constraint(Default = SystemMethods.NewGuid)]
    public Guid UniqueId { get; set; }

    [Column(ParentIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_parentId_nodeObjectType", ForColumns = $"{ParentIdColumnName},{NodeObjectTypeColumnName}", IncludeColumns = $"{TrashedColumnName},{UserIdColumnName},{LevelColumnName},{PathColumnName},{SortOrderColumnName},{UniqueIdColumnNameTypo},{TextColumnName},{CreateDateColumnName}")]
    public int ParentId { get; set; }

    // NOTE: This index is primarily for the nucache data lookup, see https://github.com/umbraco/Umbraco-CMS/pull/8365#issuecomment-673404177
    [Column(LevelColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Level", ForColumns = $"{LevelColumnName},{ParentIdColumnName},{SortOrderColumnName},{NodeObjectTypeColumnName},{TrashedColumnName}", IncludeColumns = $"{UserIdColumnName},{PathColumnName},{KeyColumnName},{CreateDateColumnName}")]
    public short Level { get; set; }

    [Column(PathColumnName)]
    [Length(150)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Path")]
    public string Path { get; set; } = null!;

    [Column(SortOrderColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_ObjectType_trashed_sorted", ForColumns = $"{NodeObjectTypeColumnName},{TrashedColumnName},{SortOrderColumnName},{IdColumnName}", IncludeColumns = $"{UniqueIdColumnNameTypo},{ParentIdColumnName},{LevelColumnName},{PathColumnName},{UserIdColumnName},{TextColumnName},{CreateDateColumnName}")]
    public int SortOrder { get; set; }

    [Column(TrashedColumnName)]
    [Constraint(Default = "0")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Trashed")]
    public bool Trashed { get; set; }

    [Column(UserIdColumnName)] // TODO: db rename to 'createUserId'
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; } // return null if zero

    [Column(TextColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Text { get; set; }

    [Column(NodeObjectTypeColumnName)] // TODO: db rename to 'objectType'
    [NullSetting(NullSetting = NullSettings.Null)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_ObjectType", ForColumns = $"{NodeObjectTypeColumnName},{TrashedColumnName}", IncludeColumns = $"{UniqueIdColumnNameTypo},{ParentIdColumnName},{LevelColumnName},{PathColumnName},{SortOrderColumnName},{UserIdColumnName},{TextColumnName},{CreateDateColumnName}")]
    public Guid? NodeObjectType { get; set; }

    [Column(CreateDateColumnName)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }
}
