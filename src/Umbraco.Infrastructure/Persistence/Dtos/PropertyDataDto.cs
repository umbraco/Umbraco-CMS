using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class PropertyDataDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyData;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string PropertyTypeIdColumnName = "propertyTypeId";
    public const string VersionIdColumnName = "versionId";
    public const int VarcharLength = 512;
    public const int SegmentLength = 256;

    private const string LanguageIdColumnName = "languageId";
    private const string SegmentColumnName = "segment";

    private decimal? _decimalValue;

    /// <summary>
    /// Gets or sets the unique primary key for this property data record.
    /// </summary>
    /// <remarks>pk, not used at the moment (never updating)</remarks>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content version to which this property data belongs.
    /// </summary>
    [Column(VersionIdColumnName)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = $"{VersionIdColumnName},{PropertyTypeIdColumnName},{LanguageIdColumnName},{SegmentColumnName}")]
    public int VersionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the property type.
    /// </summary>
    [Column(PropertyTypeIdColumnName)]
    [ForeignKey(typeof(PropertyTypeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_PropertyTypeId")]
    public int PropertyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the segment identifier used to distinguish variations of the property data, such as culture or segment-based variations in Umbraco.
    /// </summary>
    [Column(SegmentColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Segment")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(SegmentLength)]
    public string? Segment { get; set; }

    /// <summary>
    /// Gets or sets the nullable integer value associated with the property data.
    /// </summary>
    [Column("intValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? IntegerValue { get; set; }

    /// <summary>
    /// Gets or sets the decimal value stored for the property data, or <c>null</c> if not set.
    /// </summary>
    [Column("decimalValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public decimal? DecimalValue
    {
        get => _decimalValue;
        set => _decimalValue = value?.Normalize();
    }

    /// <summary>
    /// Gets or sets the nullable date and time value associated with the property data.
    /// </summary>
    [Column("dateValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? DateValue { get; set; }

    /// <summary>
    /// Gets or sets the string value stored in the <c>varcharValue</c> column for this property data record.
    /// This typically contains the value of a property when stored as a variable-length string.
    /// </summary>
    [Column("varcharValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(VarcharLength)]
    public string? VarcharValue { get; set; }

    /// <summary>
    /// Gets or sets the text value associated with the property data in the database.
    /// </summary>
    [Column("textValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string? TextValue { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PropertyTypeDto"/> associated with this property data, representing the definition of the property type.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(PropertyTypeId))]
    public PropertyTypeDto? PropertyTypeDto { get; set; }

    /// <summary>
    /// Gets the value of the property by returning the first non-null or non-empty value from the following, in order: <see cref="IntegerValue"/>, <see cref="DecimalValue"/>, <see cref="DateValue"/>, <see cref="VarcharValue"/>, and <see cref="TextValue"/>.
    /// </summary>
    [Ignore]
    public object? Value
    {
        get
        {
            if (IntegerValue.HasValue)
            {
                return IntegerValue.Value;
            }

            if (DecimalValue.HasValue)
            {
                return DecimalValue.Value;
            }

            if (DateValue.HasValue)
            {
                return DateValue.Value;
            }

            if (!string.IsNullOrEmpty(VarcharValue))
            {
                return VarcharValue;
            }

            if (!string.IsNullOrEmpty(TextValue))
            {
                return TextValue;
            }

            return null;
        }
    }

    /// <summary>
    /// Creates a copy of the current <see cref="PropertyDataDto"/> with the specified version ID.
    /// </summary>
    /// <param name="versionId">The version ID to assign to the cloned instance.</param>
    /// <returns>A new <see cref="PropertyDataDto"/> instance with the given version ID and copied property values.</returns>
    public PropertyDataDto Clone(int versionId) =>
        new PropertyDataDto
        {
            VersionId = versionId,
            PropertyTypeId = PropertyTypeId,
            LanguageId = LanguageId,
            Segment = Segment,
            IntegerValue = IntegerValue,
            DecimalValue = DecimalValue,
            DateValue = DateValue,
            VarcharValue = VarcharValue,
            TextValue = TextValue,
            PropertyTypeDto = PropertyTypeDto,
        };

    private bool Equals(PropertyDataDto other) => Id == other.Id;

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="PropertyDataDto"/> instance.
    /// Equality is based on the <c>Id</c> property value.
    /// </summary>
    /// <param name="other">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="PropertyDataDto"/> and has the same <c>Id</c>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? other) =>
        !ReferenceEquals(null, other) // other is not null
        && (ReferenceEquals(this, other) // and either ref-equals, or same id
            || (other is PropertyDataDto pdata && pdata.Id == Id));

    /// <summary>
    /// Returns a hash code for this instance, based on the <see cref="Id"/> property.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode() =>

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        Id;
}
