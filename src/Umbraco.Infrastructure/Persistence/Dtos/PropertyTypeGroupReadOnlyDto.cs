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

    [Column(PrimaryKeyColumnName)]
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
