using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(TemplateDtoConfiguration))]
public class TemplateDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Template;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string AliasColumnName = "alias";

    /// <summary>
    /// Gets or sets the primary key of the template.
    /// </summary>
    public int PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the node associated with this template.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the alias of the template.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the node associated with this template.
    /// </summary>
    public NodeDto NodeDto { get; set; } = null!;
}
