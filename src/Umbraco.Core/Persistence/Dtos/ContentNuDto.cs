using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.NodeData)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class ContentNuDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsContentNu", OnColumns = "nodeId, published")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        public int NodeId { get; set; }

        [Column("published")]
        public bool Published { get; set; }

        [Column("data")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string Data { get; set; }

        [Column("rv")]
        public long Rv { get; set; }
    }
}
