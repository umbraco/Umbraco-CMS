using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DocumentVersionDtoConfiguration))]
public class DocumentVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string TemplateIdColumnName = "templateId";
    public const string PublishedColumnName = "published";

    /// <summary>
    /// Gets or sets the unique identifier for the document version.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the template identifier associated with the document version.
    /// </summary>
    public int? TemplateId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document version is published.
    /// </summary>
    public bool Published { get; set; }
}
