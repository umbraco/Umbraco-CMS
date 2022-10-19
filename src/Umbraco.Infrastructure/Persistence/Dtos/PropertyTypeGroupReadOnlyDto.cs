using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.PropertyTypeGroup)]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
internal class PropertyTypeGroupReadOnlyDto
{
    [Column("PropertyTypeGroupId")]
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
