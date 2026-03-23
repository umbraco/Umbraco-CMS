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

    // To avoid any risk of casing bugs caused by inconsistencies between upgraded and new installs, we keep the casing "Id" here
    // even though in other tables the usual casing is lower-case ("id").
    public const string IdKeyColumnName = "Id";

    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string SortOrderColumnName = "SortOrder";
    public const string AllowedIdColumnName = "AllowedId";

    /// <summary>
    /// Gets or sets the identifier of the parent content type in the allowed content type relationship.
    /// </summary>
    [Column(IdKeyColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType", Column = ContentTypeDto.NodeIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_cmsContentTypeAllowedContentType", OnColumns = $"{IdKeyColumnName}, {AllowedIdColumnName}")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type that is allowed by the parent content type.
    /// </summary>
    [Column(AllowedIdColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType1", Column = ContentTypeDto.NodeIdColumnName)]
    public int AllowedId { get; set; }

    /// <summary>
    /// Gets or sets the order in which the allowed content type appears.
    /// </summary>
    [Column(SortOrderColumnName)]
    [Constraint(Name = "df_cmsContentTypeAllowedContentType_sortOrder", Default = "0")]
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the content type that is allowed by this relationship.
    /// Represents the related <see cref="ContentTypeDto"/> entity in a one-to-one association.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentTypeDto? ContentTypeDto { get; set; }
}
