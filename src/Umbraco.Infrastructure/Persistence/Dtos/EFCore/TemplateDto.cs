using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(TemplateDtoConfiguration))]
public sealed class TemplateDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Template;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    public int PrimaryKey { get; set; }

    public int NodeId { get; set; }

    public string? Alias { get; set; }

    public NodeDto NodeDto { get; set; } = null!;
}
