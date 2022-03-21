using NPoco;

namespace Umbraco.Cms.Persistence.SqlServer.Dtos
{
    internal class DefinedIndexDto
    {

        [Column("TABLE_NAME")]
        public string TableName { get; set; }

        [Column("INDEX_NAME")]
        public string IndexName { get; set; }

        [Column("COLUMN_NAME")]
        public string ColumnName { get; set; }

        [Column("UNIQUE")]
        public short Unique { get; set; }
    }
}
