using System;
using System.Data;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("contentTypeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class ContentVersionCleanupPolicyDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCleanupPolicy;

        [Column("contentTypeId")]
        [ForeignKey(typeof(ContentTypeDto), Column = "nodeId", OnDelete = Rule.Cascade)]
        public int ContentTypeId { get; set; }

        [Column("keepAllVersionsNewerThanDays")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? KeepAllVersionsNewerThanDays { get; set; }

        [Column("keepLatestVersionPerDayForDays")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? KeepLatestVersionPerDayForDays { get; set; }

        [Column("preventCleanup")]
        public bool PreventCleanup { get; set; }

        [Column("updated")]
        public DateTime Updated { get; set; }
    }
}
