using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(PropertyTypeDtoConfiguration))]
public class PropertyTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string DataTypeIdColumnName = "dataTypeId";
    public const string ContentTypeIdColumnName = "contentTypeId";
    public const string PropertyTypeGroupIdColumnName = "propertyTypeGroupId";

    /// <summary>Gets or sets the unique identifier for the property type.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the data type associated with this property type.</summary>
    public int DataTypeId { get; set; }

    /// <summary>Gets or sets the identifier of the content type associated with this property type.</summary>
    public int ContentTypeId { get; set; }

    /// <summary>Gets or sets the ID of the property type group this property type belongs to.</summary>
    public int? PropertyTypeGroupId { get; set; }

    /// <summary>Gets or sets the unique alias that identifies the property type.</summary>
    public string? Alias { get; set; }

    /// <summary>Gets or sets the name of the property type.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the sort order index for this property type.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets a value indicating whether this property type is mandatory.</summary>
    public bool Mandatory { get; set; }

    /// <summary>Gets or sets the validation message displayed when a mandatory property is not provided.</summary>
    public string? MandatoryMessage { get; set; }

    /// <summary>Gets or sets the validation regular expression for the property type.</summary>
    public string? ValidationRegExp { get; set; }

    /// <summary>Gets or sets the message displayed when the validation regular expression fails.</summary>
    public string? ValidationRegExpMessage { get; set; }

    /// <summary>Gets or sets an optional description for the property type.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets a value indicating whether the property's label is displayed above the input field.</summary>
    public bool LabelOnTop { get; set; }

    /// <summary>Gets or sets the variation flags for the property type.</summary>
    public byte Variations { get; set; }

    /// <summary>Gets or sets the unique GUID identifier for the property type.</summary>
    public Guid UniqueId { get; set; }
}
