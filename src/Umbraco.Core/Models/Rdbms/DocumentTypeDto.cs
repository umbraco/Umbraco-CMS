using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDocumentType")]
    [PrimaryKey("contentTypeNodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class DocumentTypeDto
    {
        [Column("contentTypeNodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("templateNodeId")]
        public int TemplateNodeId { get; set; }

        [Column("IsDefault")]
        public bool IsDefault { get; set; }

        [ResultColumn]
        public ContentTypeDto ContentTypeDto { get; set; }
    }
}