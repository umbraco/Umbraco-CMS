using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsStylesheet")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    public class StylesheetDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("filename")]
        [DatabaseType(SpecialDbTypes.NVARCHAR, Length = 100)]
        public string Filename { get; set; }

        [Column("content")]
        [DatabaseType(SpecialDbTypes.NTEXT)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Content { get; set; }
    }
}