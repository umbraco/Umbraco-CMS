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

    /// <summary>
    /// Gets or sets the <see cref="NodeDto"/> of the parent content type.
    /// </summary>
    /// <remarks>
    /// The parent/child columns are foreign keys to <c>umbracoNode</c>, so the navigations target
    /// <see cref="NodeDto"/> (matching the physical constraints) rather than <see cref="ContentTypeDto"/>.
    /// </remarks>
    public NodeDto ParentNode { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="NodeDto"/> of the child content type.
    /// </summary>
    public NodeDto ChildNode { get; set; } = null!;
}
