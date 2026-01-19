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

    internal const string ReferenceColumnName = "PropertyTypeId"; // should be PropertyTypeId, but for database compatibility we keep it like this

    private const string LanguageIdColumnName = "languageId";
    private const string SegmentColumnName = "segment";

    private decimal? _decimalValue;

    // pk, not used at the moment (never updating)
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(VersionIdColumnName)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = $"{VersionIdColumnName},{PropertyTypeIdColumnName},{LanguageIdColumnName},{SegmentColumnName}")]
    public int VersionId { get; set; }

    [Column(PropertyTypeIdColumnName)]
    [ForeignKey(typeof(PropertyTypeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_PropertyTypeId")]
    public int PropertyTypeId { get; set; }

    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LanguageId { get; set; }

    [Column(SegmentColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Segment")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(SegmentLength)]
    public string? Segment { get; set; }

    [Column("intValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? IntegerValue { get; set; }

    [Column("decimalValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public decimal? DecimalValue
    {
        get => _decimalValue;
        set => _decimalValue = value?.Normalize();
    }

    [Column("dateValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? DateValue { get; set; }

    [Column("varcharValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(VarcharLength)]
    public string? VarcharValue { get; set; }

    [Column("textValue")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string? TextValue { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = ReferenceColumnName)]
    public PropertyTypeDto? PropertyTypeDto { get; set; }

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

    public override bool Equals(object? other) =>
        !ReferenceEquals(null, other) // other is not null
        && (ReferenceEquals(this, other) // and either ref-equals, or same id
            || (other is PropertyDataDto pdata && pdata.Id == Id));

    public override int GetHashCode() =>

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        Id;
}
