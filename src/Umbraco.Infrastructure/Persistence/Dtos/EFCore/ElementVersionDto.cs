using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ElementVersionDtoConfiguration))]
public class ElementVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ElementVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string PublishedColumnName = "published";

    /// <summary>
    /// Gets or sets the unique identifier for the element version.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this element version is published.
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets the shared content version row. Not a database column — populated by the repository after query.
    /// </summary>
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
