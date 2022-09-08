using NPoco;

namespace Umbraco.Cms.Persistence.SqlServer.Dtos;

internal class ConstraintPerColumnDto
{
    [Column("TABLE_NAME")]
    public string TableName { get; set; } = null!;

    [Column("COLUMN_NAME")]
    public string ColumnName { get; set; } = null!;

    [Column("CONSTRAINT_NAME")]
    public string ConstraintName { get; set; } = null!;
}
