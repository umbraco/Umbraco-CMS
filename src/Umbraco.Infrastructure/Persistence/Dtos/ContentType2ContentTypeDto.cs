using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.ElementTypeTree)]
[ExplicitColumns]
internal class ContentType2ContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ElementTypeTree;

    [Column("parentContentTypeId")]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_cmsContentType2ContentType", OnColumns = "parentContentTypeId, childContentTypeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_parent")]
    public int ParentId { get; set; }

    [Column("childContentTypeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_child")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_childContentTypeId", ForColumns = "childContentTypeId,parentContentTypeId")]
    public int ChildId { get; set; }
}
