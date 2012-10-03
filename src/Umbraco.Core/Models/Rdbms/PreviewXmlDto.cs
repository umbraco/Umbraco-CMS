using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPreviewXml")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class PreviewXmlDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("versionId")]
        public Guid VersionId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("xml")]
        public string Xml { get; set; }
    }
}