using NPoco;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    [TableName("cmsContentNu")]
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
