using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPreviewXml")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class PreviewXmlDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "", OnColumns = "[nodeId], [versionId]")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        public int NodeId { get; set; }

        [Column("versionId")]
        [ForeignKey(typeof(ContentVersionDto), Column = "VersionId")]
        public Guid VersionId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("xml")]
        [DatabaseType(DatabaseTypes.Ntext)]
        public string Xml { get; set; }
    }
}