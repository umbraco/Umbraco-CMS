using NPoco;

namespace Umbraco.Core.Persistence.Dtos
{
    public class TableInSchemaDto
    {
        [Column("TABLE_NAME")]
        public string TableName { get; set; }
    }
}
