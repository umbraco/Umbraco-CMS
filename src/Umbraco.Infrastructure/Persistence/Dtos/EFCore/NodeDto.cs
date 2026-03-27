using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(NodeDtoConfiguration))]
public class NodeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Node;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    // Public constants to bind properties between DTOs
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

    private int? _userId;

    /// <summary>
    /// Gets or sets the unique identifier for the node.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the globally unique identifier (GUID) that uniquely identifies this node.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent node.
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// Gets or sets the depth level of the node within the content tree hierarchy.
    /// </summary>
    public short Level { get; set; }

    /// <summary>
    /// Gets or sets the hierarchical path of the node.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the position of the node relative to its siblings.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this node is in the trash.
    /// </summary>
    public bool Trashed { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the node.
    /// </summary>
    public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; }

    /// <summary>
    /// Gets or sets the display text or name associated with the node.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the object type GUID associated with this node.
    /// </summary>
    public Guid? NodeObjectType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the node was created.
    /// </summary>
    public DateTime CreateDate { get; set; }
}
