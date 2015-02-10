using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDocument")]
    [PrimaryKey("versionId", autoIncrement = false)]
    [ExplicitColumns]
    internal class DocumentPublishedReadOnlyDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("published")]
        public bool Published { get; set; }

        [Column("versionId")]
        public Guid VersionId { get; set; }

        [Column("newest")]
        public bool Newest { get; set; }
    }
}