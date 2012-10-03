using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDataTypePreValues")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class DataTypePreValueDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("datatypeNodeId")]
        public int DataTypeNodeId { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("sortorder")]
        public int SortOrder { get; set; }

        [Column("alias")]
        public string Alias { get; set; }
    }
}