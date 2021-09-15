using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos
{
    internal class ConstraintPerColumnDto
    {
        [Column("TABLE_NAME")]
        public string TableName { get; set; }

        [Column("COLUMN_NAME")]
        public string ColumnName { get; set; }

        [Column("CONSTRAINT_NAME")]
        public string ConstraintName { get; set; }
    }
}
