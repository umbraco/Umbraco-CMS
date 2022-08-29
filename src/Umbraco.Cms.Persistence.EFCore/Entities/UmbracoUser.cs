using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoUser
    {
        public UmbracoUser()
        {
            UmbracoContentVersionCultureVariations = new HashSet<UmbracoContentVersionCultureVariation>();
            UmbracoContentVersions = new HashSet<UmbracoContentVersion>();
            UmbracoLogs = new HashSet<UmbracoLog>();
            UmbracoNodes = new HashSet<UmbracoNode>();
            UmbracoUser2NodeNotifies = new HashSet<UmbracoUser2NodeNotify>();
            UmbracoUserLogins = new HashSet<UmbracoUserLogin>();
            UmbracoUserStartNodes = new HashSet<UmbracoUserStartNode>();
            UserGroups = new HashSet<UmbracoUserGroup>();
        }

        public int Id { get; set; }
        public bool? UserDisabled { get; set; }
        public bool? UserNoConsole { get; set; }
        public string UserName { get; set; } = null!;
        public string UserLogin { get; set; } = null!;
        public string UserPassword { get; set; } = null!;
        public string? PasswordConfig { get; set; }
        public string UserEmail { get; set; } = null!;
        public string? UserLanguage { get; set; }
        public string? SecurityStampToken { get; set; }
        public int? FailedLoginAttempts { get; set; }
        public DateTime? LastLockoutDate { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? EmailConfirmedDate { get; set; }
        public DateTime? InvitedDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? Avatar { get; set; }
        public string? TourData { get; set; }

        public virtual ICollection<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; }
        public virtual ICollection<UmbracoContentVersion> UmbracoContentVersions { get; set; }
        public virtual ICollection<UmbracoLog> UmbracoLogs { get; set; }
        public virtual ICollection<UmbracoNode> UmbracoNodes { get; set; }
        public virtual ICollection<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; set; }
        public virtual ICollection<UmbracoUserLogin> UmbracoUserLogins { get; set; }
        public virtual ICollection<UmbracoUserStartNode> UmbracoUserStartNodes { get; set; }

        public virtual ICollection<UmbracoUserGroup> UserGroups { get; set; }
    }
}
