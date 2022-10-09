using NPoco;

namespace Umbraco.Cms.Persistence.SqlServer.Dtos;

internal class DefinedIndexDto
{
    [Column("TABLE_NAME")]
    public string TableName { get; set; } = null!;

    [Column("INDEX_NAME")]
    public string IndexName { get; set; } = null!;

    [Column("COLUMN_NAME")]
    public string ColumnName { get; set; } = null!;

    [Column("UNIQUE")]
    public short Unique { get; set; }
}
