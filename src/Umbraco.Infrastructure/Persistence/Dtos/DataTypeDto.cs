using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.DataType)]
[PrimaryKey("nodeId", AutoIncrement = false)]
[ExplicitColumns]
public class DataTypeDto
{
    [Column("nodeId")]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    [Column("propertyEditorAlias")]
    public string EditorAlias { get; set; } = null!; // TODO: should this have a length

    [Column("dbType")]
    [Length(50)]
    public string DbType { get; set; } = null!;

    [Column("config")]
    [SpecialDbType(SpecialDbTypes.NTEXT)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Configuration { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = "NodeId")]
    public NodeDto NodeDto { get; set; } = null!;
}
