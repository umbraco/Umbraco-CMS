using NPoco;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
[TableName("cmsDataTypePreValues")]
[ExplicitColumns]
public class PreValueDto
{
    [Column("id")]
    public int Id { get; set; }

    [Column("datatypeNodeId")]
    public int NodeId { get; set; }

    [Column("alias")]
    public string Alias { get; set; } = null!;

    [Column("sortorder")]
    public int SortOrder { get; set; }

    [Column("value")]
    public string? Value { get; set; }
}
