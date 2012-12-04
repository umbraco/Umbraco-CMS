using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDocumentType")]
    [PrimaryKey("contentTypeNodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class DocumentTypeDto
    {
        [Column("contentTypeNodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsDocumentType", OnColumns = "contentTypeNodeId, templateNodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("templateNodeId")]
        public int TemplateNodeId { get; set; }

        [Column("IsDefault")]
        [Constraint(Default = "0")]
        public bool IsDefault { get; set; }

        [ResultColumn]
        public ContentTypeDto ContentTypeDto { get; set; }
    }
}