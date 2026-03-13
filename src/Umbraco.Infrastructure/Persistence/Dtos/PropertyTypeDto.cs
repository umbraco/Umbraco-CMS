using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal class PropertyTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string PropertyTypeGroupIdColumnName = "propertyTypeGroupId";
    public const string DataTypeIdColumnName = "dataTypeId";
    public const string ContentTypeIdColumnName = "contentTypeId";

    private string? _alias;

    /// <summary>
    /// Gets or sets the unique identifier for the property type.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = 100)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the data type associated with this property type.
    /// </summary>
    [Column(DataTypeIdColumnName)]
    [ForeignKey(typeof(DataTypeDto), Column = DataTypeDto.PrimaryKeyColumnName)]
    public int DataTypeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type associated with this property type.
    /// </summary>
    [Column(ContentTypeIdColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int ContentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the property type group this property type belongs to.
    /// </summary>
    [Column(PropertyTypeGroupIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(PropertyTypeGroupDto))]
    public int? PropertyTypeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the unique alias that identifies the property type.
    /// </summary>
    [Index(IndexTypes.NonClustered, Name = "IX_cmsPropertyTypeAlias")]
    [Column("Alias")]
    public string? Alias { get => _alias; set => _alias = value == null ? null : string.Intern(value); }

    /// <summary>
    /// Gets or sets the name of the property type.
    /// </summary>
    [Column("Name")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the zero-based sort order index for this property type within its containing collection.
    /// </summary>
    [Column("sortOrder")]
    [Constraint(Default = "0")]
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property type is mandatory.
    /// </summary>
    [Column("mandatory")]
    [Constraint(Default = "0")]
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the custom validation message displayed when a mandatory property of this type is not provided.
    /// </summary>
    [Column("mandatoryMessage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? MandatoryMessage { get; set; }

    /// <summary>
    /// Gets or sets the validation regular expression for the property type.
    /// </summary>
    [Column("validationRegExp")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string? ValidationRegExp { get; set; }

    /// <summary>
    /// Gets or sets the message displayed when the validation regular expression fails.
    /// </summary>
    [Column("validationRegExpMessage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? ValidationRegExpMessage { get; set; }

    /// <summary>
    /// Gets or sets an optional description for the property type.
    /// </summary>
    [Column("Description")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property's label is displayed above the input field in the UI.
    /// </summary>
    [Column("labelOnTop")]
    [Constraint(Default = "0")]
    public bool LabelOnTop { get; set; }

    /// <summary>
    /// Gets or sets the variation flags for the property type, indicating whether the property supports culture, segment, or invariant variations.
    /// The value corresponds to the <c>ContentVariation</c> enum.
    /// </summary>
    [Column("variations")]
    [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
    public byte Variations { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DataTypeDto"/> that is associated with this <see cref="PropertyTypeDto"/>.
    /// This represents a one-to-one relationship based on the <c>DataTypeId</c> column.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(DataTypeId))]
    public DataTypeDto DataTypeDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the property type.
    /// </summary>
    [Column("UniqueId")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeUniqueID")]
    public Guid UniqueId { get; set; }
}
