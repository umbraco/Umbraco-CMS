using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoCacheInstruction")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class CacheInstructionDto
    {
        [Column("id")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [PrimaryKeyColumn(AutoIncrement = true, Name = "PK_umbracoCacheInstruction")]
        public int Id { get; set; }

        [Column("utcStamp")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime UtcStamp { get; set; }

        [Column("jsonInstruction")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Instructions { get; set; }

        [Column("originated")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(500)]
        public string OriginIdentity { get; set; }
    }
}