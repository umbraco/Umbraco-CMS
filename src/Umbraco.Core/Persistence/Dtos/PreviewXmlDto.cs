using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.NodePreviewXml)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class PreviewXmlDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        public int NodeId { get; set; }

        [Column("xml")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string Xml { get; set; }

        [Column("rv")]
        public long Rv { get; set; }
    }
}
