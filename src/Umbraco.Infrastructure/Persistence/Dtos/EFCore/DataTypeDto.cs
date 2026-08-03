using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DataTypeDtoConfiguration))]
public class DataTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DataType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string EditorAliasColumnName = "propertyEditorAlias";
    public const string EditorUiAliasColumnName = "propertyEditorUiAlias";
    public const string DbTypeColumnName = "dbType";
    public const string ConfigurationColumnName = "config";

    /// <summary>
    /// Gets or sets the identifier of the associated node. Also the primary key.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the alias of the property editor associated with this data type.
    /// </summary>
    public string EditorAlias { get; set; } = null!;

    /// <summary>
    /// Gets or sets the alias of the editor UI associated with the data type.
    /// </summary>
    public string? EditorUiAlias { get; set; }

    /// <summary>
    /// Gets or sets the type of the database column used to store values for this data type.
    /// </summary>
    public string DbType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the serialized configuration for the data type.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="EFCore.NodeDto"/> associated with this data type.
    /// </summary>
    public NodeDto NodeDto { get; set; } = null!;
}
