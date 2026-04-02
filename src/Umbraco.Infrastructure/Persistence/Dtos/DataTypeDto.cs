using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data Transfer Object (DTO) for persisting data type information in the database.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class DataTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DataType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    /// <summary>
    /// Gets or sets the identifier of the associated node.
    /// This value is a foreign key to the <see cref="NodeDto"/> entity.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the alias of the property editor associated with this data type.
    /// </summary>
    /// <remarks>TODO: should this have a length</remarks>
    [Column("propertyEditorAlias")]
    public string EditorAlias { get; set; } = null!;

    /// <summary>
    /// Gets or sets the alias of the editor UI associated with the data type.
    /// </summary>
    [Column("propertyEditorUiAlias")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? EditorUiAlias { get; set; }

    /// <summary>
    /// Gets or sets the type of the database column used to store values for this data type.
    /// </summary>
    [Column("dbType")]
    [Length(50)]
    public string DbType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the serialized configuration for the data type.
    /// This is typically stored as a JSON string.
    /// </summary>
    [Column("config")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.NodeDto"/> associated with this data type.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(NodeId))]
    public NodeDto NodeDto { get; set; } = null!;
}
