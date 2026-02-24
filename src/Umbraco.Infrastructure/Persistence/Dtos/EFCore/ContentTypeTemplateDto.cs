using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentTypeTemplateDtoConfiguration))]
public sealed class ContentTypeTemplateDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentType;
    public const string ContentTypeNodeIdColumnName = "contentTypeNodeId";
    public const string TemplateNodeIdColumnName = "templateNodeId";
    public const string IsDefaultColumnName = "IsDefault"; // capital I and D, preserved from NPoco

    public int ContentTypeNodeId { get; set; }

    public int TemplateNodeId { get; set; }

    public bool IsDefault { get; set; }

    // Navigation to TemplateDto (in EFCore model)
    public TemplateDto TemplateDto { get; set; } = null!;

    // TODO: Add navigation to ContentTypeDto when ContentTypeDto is migrated to EFCore
}
