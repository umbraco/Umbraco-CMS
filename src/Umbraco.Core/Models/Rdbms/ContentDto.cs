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
        [NullSetting(NullSetting = NullSettings.Null)]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContent")]
        public int NodeId { get; set; }

        [Column("contentType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int ContentType { get; set; }

        [ResultColumn]
        public NodeDto NodeDto { get; set; }
    }
}