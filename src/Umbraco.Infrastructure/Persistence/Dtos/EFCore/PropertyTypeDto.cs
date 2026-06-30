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
    public const string AliasColumnName = "Alias";
    public const string NameColumnName = "Name";
    public const string SortOrderColumnName = "sortOrder";
    public const string MandatoryColumnName = "mandatory";
    public const string MandatoryMessageColumnName = "mandatoryMessage";
    public const string ValidationRegExpColumnName = "validationRegExp";
    public const string ValidationRegExpMessageColumnName = "validationRegExpMessage";
    public const string DescriptionColumnName = "Description";
    public const string LabelOnTopColumnName = "labelOnTop";
    public const string VariationsColumnName = "variations";
    public const string UniqueIdColumnName = "UniqueId";

    private string? _alias;

    /// <summary>
    /// Gets or sets the unique identifier (primary key) for the property type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the data type associated with this property type.
    /// </summary>
    public int DataTypeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type associated with this property type.
    /// </summary>
    public int ContentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the property type group this property type belongs to.
    /// </summary>
    public int? PropertyTypeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the unique alias that identifies the property type.
    /// </summary>
    public string? Alias { get => _alias; set => _alias = value == null ? null : string.Intern(value); }

    /// <summary>
    /// Gets or sets the name of the property type.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the zero-based sort order index for this property type within its containing collection.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property type is mandatory.
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the custom validation message displayed when a mandatory property of this type is not provided.
    /// </summary>
    public string? MandatoryMessage { get; set; }

    /// <summary>
    /// Gets or sets the validation regular expression for the property type.
    /// </summary>
    public string? ValidationRegExp { get; set; }

    /// <summary>
    /// Gets or sets the message displayed when the validation regular expression fails.
    /// </summary>
    public string? ValidationRegExpMessage { get; set; }

    /// <summary>
    /// Gets or sets an optional description for the property type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property's label is displayed above the input field in the UI.
    /// </summary>
    public bool LabelOnTop { get; set; }

    /// <summary>
    /// Gets or sets the variation flags for the property type.
    /// </summary>
    public byte Variations { get; set; }

    /// <summary>
    /// Gets or sets the GUID that uniquely identifies the property type.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="EFCore.DataTypeDto"/> associated with this property type.
    /// </summary>
    public DataTypeDto DataTypeDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="EFCore.PropertyTypeGroupDto"/> this property type belongs to, if any.
    /// </summary>
    public PropertyTypeGroupDto? PropertyTypeGroupDto { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="EFCore.MemberPropertyTypeDto"/> extending this property type with member-specific
    /// metadata, if any (only present for property types on member types).
    /// </summary>
    public MemberPropertyTypeDto? MemberPropertyTypeDto { get; set; }
}
