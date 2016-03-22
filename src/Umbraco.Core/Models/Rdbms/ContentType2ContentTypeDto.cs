using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentType2ContentType")]
    [ExplicitColumns]
    internal class ContentType2ContentTypeDto
    {
        [Column("parentContentTypeId")]
        [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_cmsContentType2ContentType", OnColumns = "parentContentTypeId, childContentTypeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_parent")]
        public int ParentId { get; set; }

        [Column("childContentTypeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_child")]
        public int ChildId { get; set; }
    }
}