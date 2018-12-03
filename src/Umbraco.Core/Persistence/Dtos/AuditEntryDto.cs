using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.AuditEntry)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class AuditEntryDto
    {
        public const int IpLength = 64;
        public const int EventTypeLength = 256;
        public const int DetailsLength = 1024;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        // there is NO foreign key to the users table here, neither for performing user nor for
        // affected user, so we can delete users and NOT delete the associated audit trails, and
        // users can still be identified via the details free-form text fields.

        [Column("performingUserId")]
        public int PerformingUserId { get; set; }

        [Column("performingDetails")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(DetailsLength)]
        public string PerformingDetails { get; set; }

        [Column("performingIp")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(IpLength)]
        public string PerformingIp { get; set; }

        [Column("eventDateUtc")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime EventDateUtc { get; set; }

        [Column("affectedUserId")]
        public int AffectedUserId { get; set; }

        [Column("affectedDetails")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(DetailsLength)]
        public string AffectedDetails { get; set; }

        [Column("eventType")]
        [Length(EventTypeLength)]
        public string EventType { get; set; }

        [Column("eventDetails")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(DetailsLength)]
        public string EventDetails { get; set; }
    }
}
