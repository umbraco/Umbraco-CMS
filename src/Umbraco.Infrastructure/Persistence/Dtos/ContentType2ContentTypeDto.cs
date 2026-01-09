using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
internal sealed class ContentType2ContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentTypeTree;
    public const string PrimaryKeyName = "parentContentTypeId";
    public const string ChildIdName = "childContentTypeId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_cmsContentType2ContentType", OnColumns = $"{PrimaryKeyName}, {ChildIdName}")]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_parent")]
    public int ParentId { get; set; }

    [Column(ChildIdName)]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_child")]
    public int ChildId { get; set; }
}
