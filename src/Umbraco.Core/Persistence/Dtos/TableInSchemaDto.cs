using NPoco;

namespace Umbraco.Core.Persistence.Dtos
{
    internal class TableInSchemaDto
    {
        [Column("TABLE_NAME")]
        public string TableName { get; set; }
    }
}
