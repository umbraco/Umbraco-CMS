﻿using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.Models
{
    /// <summary>
    /// Snapshot of the <see cref="PropertyDataDto"/> as it was at version 8.0
    /// </summary>
    /// <remarks>
    /// This is required during migrations the schema of this table changed and running SQL against the new table would result in errors
    /// </remarks>
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyDataDto80
    {
        public const string TableName = Constants.DatabaseSchema.Tables.PropertyData;
        public const int VarcharLength = 512;
        public const int SegmentLength = 256;

        private decimal? _decimalValue;

        // pk, not used at the moment (never updating)
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("versionId")]
        [ForeignKey(typeof(ContentVersionDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = "versionId,propertyTypeId,languageId,segment")]
        public int VersionId { get; set; }

        [Column("propertyTypeId")]
        [ForeignKey(typeof(PropertyTypeDto80))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_PropertyTypeId")]
        public int PropertyTypeId { get; set; }

        [Column("languageId")]
        [ForeignKey(typeof(LanguageDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? LanguageId { get; set; }

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
        public PropertyTypeDto80 PropertyTypeDto { get; set; }

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

        public PropertyDataDto80 Clone(int versionId)
        {
            return new PropertyDataDto80
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
                PropertyTypeDto = PropertyTypeDto
            };
        }

        protected bool Equals(PropertyDataDto other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object other)
        {
            return
                !ReferenceEquals(null, other) // other is not null
                && (ReferenceEquals(this, other) // and either ref-equals, or same id
                    || other is PropertyDataDto pdata && pdata.Id == Id);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id;
        }
    }
}
