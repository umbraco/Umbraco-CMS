using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([IdKeyColumnName, AllowedIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ContentTypeAllowedContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentChildType;
    public const string PrimaryKeyColumnName = "PK_cmsContentTypeAllowedContentType";

    // To avoid any risk of casing bugs caused by inconsistencies between upgraded and new installs, we keep the casing "Id" here even though in other tables the usual casing is lower-case ("id").
    public const string IdKeyColumnName = "Id";
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string SortOrderColumnName = "SortOrder";
    public const string AllowedIdColumnName = "AllowedId";

    [Column(IdKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = PrimaryKeyColumnName, OnColumns = $"{IdKeyColumnName}, {AllowedIdColumnName}")]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType", Column = ContentTypeDto.NodeIdColumnName)]
    public int Id { get; set; }

    [Column(AllowedIdColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType1", Column = ContentTypeDto.NodeIdColumnName)]
    public int AllowedId { get; set; }

    [Column(SortOrderColumnName)]
    [Constraint(Name = "df_cmsContentTypeAllowedContentType_sortOrder", Default = "0")]
    public int SortOrder { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentTypeDto? ContentTypeDto { get; set; }
}
