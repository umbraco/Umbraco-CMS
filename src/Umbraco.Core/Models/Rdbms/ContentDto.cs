using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContent")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class ContentDto
    {
        [Column("pk")]
        [PrimaryKeyColumn]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContent")]
        public int NodeId { get; set; }

        [Column("contentType")]
        [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
        public int ContentTypeId { get; set; }

        [ResultColumn]
        public NodeDto NodeDto { get; set; }
    }
}