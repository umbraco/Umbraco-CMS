using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
/// Represents a row in the template table, which holds the alias of each template node.
/// </summary>
[EntityTypeConfiguration(typeof(TemplateDtoConfiguration))]
public class TemplateDto
{
    /// <summary>
    /// The name of the template table.
    /// </summary>
    public const string TableName = Constants.DatabaseSchema.Tables.Template;

    /// <summary>
    /// The name of the primary key column.
    /// </summary>
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;

    /// <summary>
    /// The name of the column holding the template's node identifier.
    /// </summary>
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    /// <summary>
    /// The name of the column holding the template alias.
    /// </summary>
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
