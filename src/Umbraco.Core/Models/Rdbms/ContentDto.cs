using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContent")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class ContentDto
    {
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("contentType")]
        public int ContentType { get; set; }

        [ResultColumn]
        public NodeDto NodeDto { get; set; }
    }
}