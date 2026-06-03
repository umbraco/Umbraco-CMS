using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentType2ContentTypeDtoConfiguration))]
public sealed class ContentType2ContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentTypeTree;
    public const string ParentIdColumnName = "parentContentTypeId";
    public const string ChildIdColumnName = "childContentTypeId";

    /// <summary>
    /// Gets or sets the identifier of the parent content type in the relationship.
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// Gets or sets the child content type identifier.
    /// </summary>
    public int ChildId { get; set; }
}
