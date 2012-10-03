using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentVersion")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ContentVersionDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ContentId")]
        public int NodeId { get; set; }

        [Column("VersionId")]
        public Guid VersionId { get; set; }

        [Column("VersionDate")]
        public DateTime VersionDate { get; set; }

        [ResultColumn]
        public ContentDto ContentDto { get; set; }
    }
}