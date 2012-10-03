using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsStylesheet")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    public class StylesheetDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("filename")]
        public string Filename { get; set; }

        [Column("content")]
        public string Content { get; set; }
    }
}