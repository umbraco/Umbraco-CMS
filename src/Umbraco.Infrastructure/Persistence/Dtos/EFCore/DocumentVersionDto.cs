using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DocumentVersionDtoConfiguration))]
public sealed class DocumentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;
    public const string IdColumnName = "id";
    public const string TemplateIdColumnName = "templateId";
    public const string PublishedColumnName = "published";

    public int Id { get; set; }

    public int? TemplateId { get; set; }

    public bool Published { get; set; }

    // Navigation to TemplateDto (in EFCore model); nullable because TemplateId is nullable
    public TemplateDto? TemplateDto { get; set; }

    // TODO: Add navigation to ContentVersionDto when ContentVersionDto is migrated to EFCore
}
