using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[Obsolete("This class is unused in Umbraco. Scheduled for removal in Umbraco 19.")]
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class PropertyTypeReadOnlyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyType;
    public const string PrimaryKeyColumnName = "PropertyTypeId";

    /// <summary>
    /// Gets or sets the unique identifier (ID) for this property type.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the data type associated with this property type.
    /// </summary>
    [Column("dataTypeId")]
    public int DataTypeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the content type to which this property type belongs.
    /// </summary>
    [Column("contentTypeId")]
    public int ContentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the group to which this property type belongs, if any.
    /// </summary>
    [Column("PropertyTypesGroupId")]
    public int? PropertyTypeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the unique alias (identifier) of the property type.
    /// </summary>
    [Column("Alias")]
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the name of the property type.
    /// </summary>
    [Column("Name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the zero-based sort order index for this property type within its containing collection.
    /// </summary>
    [Column("PropertyTypeSortOrder")]
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property type is mandatory.
    /// </summary>
    [Column("mandatory")]
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the custom validation message displayed when the property is mandatory and not provided.
    /// </summary>
    [Column("mandatoryMessage")]
    public string? MandatoryMessage { get; set; }

    /// <summary>
    /// Gets or sets the regular expression used to validate the value of this property type.
    /// </summary>
    [Column("validationRegExp")]
    public string? ValidationRegExp { get; set; }

    /// <summary>
    /// Gets or sets the message displayed when the validation regular expression fails.
    /// </summary>
    [Column("validationRegExpMessage")]
    public string? ValidationRegExpMessage { get; set; }

    /// <summary>
    /// Gets or sets the textual description associated with this property type.
    /// </summary>
    [Column("Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property label is displayed above (on top of) the input field in the UI.
    /// </summary>
    [Column("labelOnTop")]
    public bool LabelOnTop { get; set; }

    /* cmsMemberType */

    /// <summary>
    /// Gets or sets a value indicating whether members can edit this property type.
    /// </summary>
    [Column("memberCanEdit")]
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property type is visible on a user's profile.
    /// </summary>
    [Column("viewOnProfile")]
    public bool ViewOnProfile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property type contains sensitive data.
    /// </summary>
    [Column("isSensitive")]
    public bool IsSensitive { get; set; }

    /* DataType */

    /// <summary>
    /// Gets or sets the alias identifying the property editor associated with this property type.
    /// </summary>
    [Column("propertyEditorAlias")]
    public string? PropertyEditorAlias { get; set; }

    /// <summary>Gets or sets the database type associated with the property type.</summary>
    [Column("dbType")]
    public string? DbType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the property type.
    /// </summary>
    [Column("UniqueID")]
    public Guid UniqueId { get; set; }
}
