﻿using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyData")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyDataDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("contentNodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyData_1", ForColumns = "contentNodeId,versionId,propertytypeid")]
        public int NodeId { get; set; }

        [Column("versionId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsPropertyData_2")]
        public Guid? VersionId { get; set; }

        [Column("propertytypeid")]
        [ForeignKey(typeof(PropertyTypeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsPropertyData_3")]
        public int PropertyTypeId { get; set; }

        [Column("dataInt")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Integer { get; set; }

        private decimal? _decimalValue;

        [Column("dataDecimal")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public decimal? Decimal
        {
            get
            {
                return _decimalValue;
            }
            set
            {
                // need to normalize the value (change the scaling factor and remove trailing zeroes)
                // because the underlying database probably has messed with the scaling factor.
                _decimalValue = value.HasValue ? (decimal?) value.Value.Normalize() : null;
            }
        }

        [Column("dataDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? Date { get; set; }

        [Column("dataNvarchar")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(500)]
        public string VarChar { get; set; }

        [Column("dataNtext")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string Text { get; set; }

        [ResultColumn]
        public PropertyTypeDto PropertyTypeDto { get; set; }

        [Ignore]
        public object GetValue
        {
            get
            {
                if (Integer.HasValue)
                {
                    return Integer.Value;
                }

                if (Decimal.HasValue)
                {
                    return Decimal.Value;
                }
                
                if (Date.HasValue)
                {
                    return Date.Value;
                }
                
                if (string.IsNullOrEmpty(VarChar) == false)
                {
                    return VarChar;
                }

                if (string.IsNullOrEmpty(Text) == false)
                {
                    return Text;
                }

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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyDataDto) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}