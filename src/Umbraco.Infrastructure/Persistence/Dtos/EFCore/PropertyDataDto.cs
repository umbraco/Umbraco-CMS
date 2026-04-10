using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(PropertyDataDtoConfiguration))]
public class PropertyDataDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyData;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string VersionIdColumnName = "versionId";
    public const string PropertyTypeIdColumnName = "propertyTypeId";
    public const string LanguageIdColumnName = "languageId";
    public const string SegmentColumnName = "segment";
    public const int VarcharLength = 512;
    public const int SegmentLength = 256;

    private decimal? _decimalValue;

    /// <summary>
    /// Gets or sets the unique primary key for this property data record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content version to which this property data belongs.
    /// </summary>
    public int VersionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the property type.
    /// </summary>
    public int PropertyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the segment identifier used to distinguish variations of the property data.
    /// </summary>
    public string? Segment { get; set; }

    /// <summary>
    /// Gets or sets the nullable integer value associated with the property data.
    /// </summary>
    public int? IntegerValue { get; set; }

    /// <summary>
    /// Gets or sets the decimal value stored for the property data, or <c>null</c> if not set.
    /// </summary>
    public decimal? DecimalValue
    {
        get => _decimalValue;
        set => _decimalValue = value?.Normalize();
    }

    /// <summary>
    /// Gets or sets the nullable date and time value associated with the property data.
    /// </summary>
    public DateTime? DateValue { get; set; }

    /// <summary>
    /// Gets or sets the string value stored in the <c>varcharValue</c> column for this property data record.
    /// </summary>
    public string? VarcharValue { get; set; }

    /// <summary>
    /// Gets or sets the text value associated with the property data in the database.
    /// </summary>
    public string? TextValue { get; set; }

    /// <summary>
    /// Gets or sets the sortable value associated with the property data in the database.
    /// </summary>
    public string? SortableValue { get; set; }

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
            SortableValue = SortableValue,
        };

    private bool Equals(PropertyDataDto other) => Id == other.Id;

    /// <inheritdoc />
    public override bool Equals(object? other) =>
        !ReferenceEquals(null, other)
        && (ReferenceEquals(this, other)
            || (other is PropertyDataDto pdata && pdata.Id == Id));

    /// <inheritdoc />
    public override int GetHashCode() =>
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        Id;
}
