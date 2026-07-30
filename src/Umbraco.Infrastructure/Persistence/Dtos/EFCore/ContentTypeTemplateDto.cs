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
    public const string IsDefaultColumnName = "IsDefault";

    /// <summary>
    /// Gets or sets the node ID of the associated content type.
    /// </summary>
    public int ContentTypeNodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated template node.
    /// </summary>
    public int TemplateNodeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this template is set as the default template for the content type.
    /// </summary>
    public bool IsDefault { get; set; }
}
