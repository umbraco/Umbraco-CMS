using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class AuditEntryDto
    {
        public const string TableName = "umbracoAudit";

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("performingUserId")]
        public int PerformingUserId { get; set; }

        [Column("performingDetails")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(1024)]
        public string PerformingDetails { get; set; }

        [Column("performingIp")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(64)]
        public string PerformingIp { get; set; }

        [Column("eventDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime EventDate { get; set; }

        [Column("affectedUserId")]
        public int AffectedUserId { get; set; }

        [Column("affectedDetails")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(1024)]
        public string AffectedDetails { get; set; }

        [Column("eventType")]
        [Length(256)]
        public string EventType { get; set; }

        [Column("eventDetails")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(1024)]
        public string EventDetails { get; set; }
    }
}
