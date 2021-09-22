using NPoco;

namespace Umbraco.Core.Persistence.Dtos
{
    internal class DefaultConstraintPerColumnDto
    {
        [Column("TABLE_NAME")]
        public string TableName { get; set; }

        [Column("COLUMN_NAME")]
        public string ColumnName { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("DEFINITION")]
        public string Definition { get; set; }
    }
}
