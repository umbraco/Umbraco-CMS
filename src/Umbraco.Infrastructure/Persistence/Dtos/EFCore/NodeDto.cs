using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(NodeDtoConfiguration))]
public class NodeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Node;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const int NodeIdSeed = 1060;
    public const string IdColumnName = PrimaryKeyColumnName;
    public const string KeyColumnName = Constants.DatabaseSchema.Columns.UniqueIdName;
    public const string UniqueIdColumnName = "uniqueID"; // actual DB column name (typo preserved for compatibility)
    public const string ParentIdColumnName = "parentId";
    public const string SortOrderColumnName = "sortOrder";
    public const string TrashedColumnName = "trashed";
    public const string NodeObjectTypeColumnName = "nodeObjectType";
    public const string TextColumnName = "text";
    public const string PathColumnName = "path";
    public const string LevelColumnName = "level";
    public const string UserIdColumnName = "nodeUser";
    public const string CreateDateColumnName = "createDate";

    private int? _userId;

    public int NodeId { get; set; }

    public Guid UniqueId { get; set; }

    public int ParentId { get; set; }

    public short Level { get; set; }

    public string Path { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool Trashed { get; set; }

    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; }

    public string? Text { get; set; }

    public Guid? NodeObjectType { get; set; }

    public DateTime CreateDate { get; set; }
}
