using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentTypeAllowedContentTypeDtoConfiguration))]
public sealed class ContentTypeAllowedContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentChildType;

    // To avoid any risk of casing bugs caused by inconsistencies between upgraded and new installs, we keep the casing "Id" here
    // even though in other tables the usual casing is lower-case ("id").
    public const string IdKeyColumnName = "Id";
    public const string AllowedIdColumnName = "AllowedId";
    public const string SortOrderColumnName = "SortOrder";

    /// <summary>
    /// Gets or sets the identifier of the parent content type in the allowed content type relationship.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type that is allowed by the parent content type.
    /// </summary>
    public int AllowedId { get; set; }

    /// <summary>
    /// Gets or sets the order in which the allowed content type appears.
    /// </summary>
    public int SortOrder { get; set; }
}
