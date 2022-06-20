using NPoco;

namespace Umbraco.Cms.Persistence.SqlServer.Dtos;

internal class ColumnInSchemaDto
{
    [Column("TABLE_NAME")]
    public string TableName { get; set; } = null!;

    [Column("COLUMN_NAME")]
    public string ColumnName { get; set; } = null!;

    [Column("ORDINAL_POSITION")]
    public int OrdinalPosition { get; set; }

    [Column("COLUMN_DEFAULT")]
    public string ColumnDefault { get; set; } = null!;

    [Column("IS_NULLABLE")]
    public string IsNullable { get; set; } = null!;

    [Column("DATA_TYPE")]
    public string DataType { get; set; } = null!;
}
