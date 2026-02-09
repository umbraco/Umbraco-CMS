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

    internal const string ReferenceColumnName = "NodeId"; // should be DataTypeDto.PrimaryKeyColumnName, but for database compatibility we keep it like this

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
    [Reference(ReferenceType.OneToOne, ColumnName = ReferenceColumnName)]
    public NodeDto NodeDto { get; set; } = null!;
}
