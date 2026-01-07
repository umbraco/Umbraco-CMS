using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ContentTypeAllowedContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentChildType;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;
    public const string AllowedIdName = "AllowedId";

    [Column(PrimaryKeyName)]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType", Column = "nodeId")]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_cmsContentTypeAllowedContentType", OnColumns = $"{PrimaryKeyName}, AllowedId")]
    public int Id { get; set; }

    [Column(AllowedIdName)]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType1", Column = "nodeId")]
    public int AllowedId { get; set; }

    [Column("SortOrder")]
    [Constraint(Name = "df_cmsContentTypeAllowedContentType_sortOrder", Default = "0")]
    public int SortOrder { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentTypeDto? ContentTypeDto { get; set; }
}
