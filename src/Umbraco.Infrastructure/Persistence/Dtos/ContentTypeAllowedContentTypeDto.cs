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

    // ToDo: Here we have an incosistent column name in DatabaseSchema. It should be Constants.DatabaseSchema.Columns.PrimaryKeyNameId; ("id")
    // For now we leave the databse schema as is to avoid breaking changes.
    public const string PrimaryKeyName = "Id";
    public const string NodeIdName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string AllowedIdName = "AllowedId";

    [Column(PrimaryKeyName)]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType", Column = NodeIdName)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_cmsContentTypeAllowedContentType", OnColumns = $"{PrimaryKeyName}, {AllowedIdName}")]
    public int Id { get; set; }

    [Column(AllowedIdName)]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType1", Column = NodeIdName)]
    public int AllowedId { get; set; }

    [Column("SortOrder")]
    [Constraint(Name = "df_cmsContentTypeAllowedContentType_sortOrder", Default = "0")]
    public int SortOrder { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentTypeDto? ContentTypeDto { get; set; }
}
