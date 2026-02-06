using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ContentType2ContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentTypeTree;
    public const string PrimaryKeyColumnName = "PK_cmsContentType2ContentType";
    public const string ParentIdColumnName = "parentContentTypeId";
    public const string ChildIdColumnName = "childContentTypeId";

    [Column(ParentIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = PrimaryKeyColumnName, OnColumns = $"{ParentIdColumnName}, {ChildIdColumnName}")]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_parent")]
    public int ParentId { get; set; }

    [Column(ChildIdColumnName)]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsContentType2ContentType_umbracoNode_child")]
    public int ChildId { get; set; }
}
