using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class DataTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DataType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    [Column("propertyEditorAlias")]
    public string EditorAlias { get; set; } = null!; // TODO: should this have a length

    [Column("propertyEditorUiAlias")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? EditorUiAlias { get; set; }

    [Column("dbType")]
    [Length(50)]
    public string DbType { get; set; } = null!;

    [Column("config")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Configuration { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(NodeId))]
    public NodeDto NodeDto { get; set; } = null!;
}
