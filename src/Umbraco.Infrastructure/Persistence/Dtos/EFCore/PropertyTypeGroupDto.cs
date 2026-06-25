using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(PropertyTypeGroupDtoConfiguration))]
public sealed class PropertyTypeGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string UniqueIdColumnName = "uniqueID";
    public const string ContentTypeNodeIdColumnName = "contenttypeNodeId";
    public const string TypeColumnName = "type";
    public const string TextColumnName = "text";
    public const string AliasColumnName = "alias";
    public const string SortOrderColumnName = "sortorder";

    /// <summary>
    /// Gets or sets the unique identifier (primary key) for the property type group.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the GUID that uniquely identifies the property type group.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type node associated with this property type group.
    /// </summary>
    public int ContentTypeNodeId { get; set; }

    /// <summary>
    /// Gets or sets the type identifier for the property type group.
    /// </summary>
    public short Type { get; set; }

    /// <summary>
    /// Gets or sets the name or label of the property type group.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the unique alias identifier for the property type group.
    /// </summary>
    public string Alias { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sort order of the property type group.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the collection of property types that belong to this property type group.
    /// </summary>
    public List<PropertyTypeDto> PropertyTypeDtos { get; set; } = [];
}
