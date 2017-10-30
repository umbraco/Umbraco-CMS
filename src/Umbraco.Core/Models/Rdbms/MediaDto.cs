using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMedia")]
    [PrimaryKey("versionId", autoIncrement = false)]
    [ExplicitColumns]
    internal class MediaDto
    {
        [Column("nodeId")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsMedia", ForColumns = "nodeId, versionId, mediaPath")]
        public int NodeId { get; set; }        

        [Column("versionId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid VersionId { get; set; }
        
        [ResultColumn]
        public ContentVersionDto ContentVersionDto { get; set; }

        [Column("mediaPath")]
        public string MediaPath { get; set; }
    }
}