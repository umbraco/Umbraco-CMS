using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDataTypePreValues")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class DataTypePreValueDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 6)]
        public int Id { get; set; }

        [Column("datatypeNodeId")]
        [ForeignKey(typeof(DataTypeDto), Column = "nodeId")]
        public int DataTypeNodeId { get; set; }

        [Column("value")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(2500)]
        public string Value { get; set; }

        [Column("sortorder")]
        public int SortOrder { get; set; }

        [Column("alias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(50)]
        public string Alias { get; set; }
    }
}