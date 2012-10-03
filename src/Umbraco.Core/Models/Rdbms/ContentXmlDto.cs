using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentXml")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class ContentXmlDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("xml")]
        public string Xml { get; set; }
    }
}