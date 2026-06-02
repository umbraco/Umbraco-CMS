using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(PropertyTypeGroupDtoConfiguration))]
public class PropertyTypeGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string ContentTypeNodeIdColumnName = "contenttypeNodeId";
    public const string UniqueIdColumnName = "uniqueID";

    /// <summary>Gets or sets the unique identifier for the property type group.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the unique GUID key for the property type group.</summary>
    public Guid UniqueId { get; set; }

    /// <summary>Gets or sets the identifier of the content type node associated with this group.</summary>
    public int ContentTypeNodeId { get; set; }

    /// <summary>Gets or sets the type of the property type group.</summary>
    public short Type { get; set; }

    /// <summary>Gets or sets the display name of the property type group.</summary>
    public string? Text { get; set; }

    /// <summary>Gets or sets the alias of the property type group.</summary>
    public string Alias { get; set; } = null!;

    /// <summary>Gets or sets the sort order of the property type group.</summary>
    public int SortOrder { get; set; }
}
