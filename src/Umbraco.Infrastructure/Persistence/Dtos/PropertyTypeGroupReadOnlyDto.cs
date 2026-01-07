using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class PropertyTypeGroupReadOnlyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;
    public const string PrimaryKeyName = "PropertyTypeGroupId"; // Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
    public int? Id { get; set; }

    [Column("PropertyGroupName")]
    public string? Text { get; set; }

    [Column("PropertyGroupSortOrder")]
    public int SortOrder { get; set; }

    [Column("contenttypeNodeId")]
    public int ContentTypeNodeId { get; set; }

    [Column("PropertyGroupUniqueID")]
    public Guid UniqueId { get; set; }
}
