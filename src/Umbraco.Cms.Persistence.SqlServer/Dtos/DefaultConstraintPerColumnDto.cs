using NPoco;

namespace Umbraco.Cms.Persistence.SqlServer.Dtos;

internal class DefaultConstraintPerColumnDto
{
    [Column("TABLE_NAME")]
    public string TableName { get; set; } = null!;

    [Column("COLUMN_NAME")]
    public string ColumnName { get; set; } = null!;

    [Column("NAME")]
    public string Name { get; set; } = null!;

    [Column("DEFINITION")]
    public string Definition { get; set; } = null!;
}
