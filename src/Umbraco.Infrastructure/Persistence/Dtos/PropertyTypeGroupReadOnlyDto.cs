using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[Obsolete("This class is unused in Umbraco. Scheduled for removal in Umbraco 19.")]
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class PropertyTypeGroupReadOnlyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;
    public const string PrimaryKeyColumnName = "PropertyTypeGroupId";

    /// <summary>
    /// Gets or sets the unique identifier for the property type group.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the property type group.
    /// </summary>
    [Column("PropertyGroupName")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the sort order of the property type group.
    /// </summary>
    [Column("PropertyGroupSortOrder")]
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated content type node.
    /// </summary>
    [Column("contenttypeNodeId")]
    public int ContentTypeNodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the property type group.
    /// </summary>
    [Column("PropertyGroupUniqueID")]
    public Guid UniqueId { get; set; }
}
