using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentVersion")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ContentVersionDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("ContentId")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsContentVersion_ContentId")]
        public int NodeId { get; set; }

        [Column("VersionId")]
        [Index(IndexTypes.UniqueNonClustered)]
        public Guid VersionId { get; set; }

        [Column("VersionDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime VersionDate { get; set; }

        [ResultColumn]
        public ContentDto ContentDto { get; set; }
    }
}