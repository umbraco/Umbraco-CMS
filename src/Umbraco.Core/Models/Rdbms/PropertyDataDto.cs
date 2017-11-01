using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyDataDto
    {
        private const string TableName = Constants.DatabaseSchema.Tables.PropertyData;
        public const int VarcharLength = 512;
        public const int SegmentLength = 256;

        private decimal? _decimalValue;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = "nodeId,versionId,propertyTypeId")]
        public int NodeId { get; set; }

        [Column("versionId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_VersionId")]
        public Guid? VersionId { get; set; }

        [Column("propertyTypeId")]
        [ForeignKey(typeof(PropertyTypeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_PropertyTypeId")]
        public int PropertyTypeId { get; set; }

        [Column("languageId")]
        [ForeignKey(typeof(LanguageDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int LanguageId { get; set; }

        [Column("segment")]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Segment")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(SegmentLength)]
        public string Segment { get; set; }

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
        public string VarcharValue { get; set; }

        [Column("textValue")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string TextValue { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "PropertyTypeId")]
        public PropertyTypeDto PropertyTypeDto { get; set; }

        [Ignore]
        public object Value
        {
            get
            {
                if (IntegerValue.HasValue)
                    return IntegerValue.Value;

                if (DecimalValue.HasValue)
                    return DecimalValue.Value;

                if (DateValue.HasValue)
                    return DateValue.Value;

                if (!string.IsNullOrEmpty(VarcharValue))
                    return VarcharValue;

                if (!string.IsNullOrEmpty(TextValue))
                    return TextValue;

                return null;
            }
        }

        protected bool Equals(PropertyDataDto other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PropertyDataDto) obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id;
        }
    }
}
