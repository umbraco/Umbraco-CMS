using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoUser
{
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

    public virtual ICollection<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; } = new List<UmbracoContentVersionCultureVariation>();

    public virtual ICollection<UmbracoContentVersion> UmbracoContentVersions { get; set; } = new List<UmbracoContentVersion>();

    public virtual ICollection<UmbracoLog> UmbracoLogs { get; set; } = new List<UmbracoLog>();

    public virtual ICollection<UmbracoNode> UmbracoNodes { get; set; } = new List<UmbracoNode>();

    public virtual ICollection<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; set; } = new List<UmbracoUser2NodeNotify>();

    public virtual ICollection<UmbracoUserLogin> UmbracoUserLogins { get; set; } = new List<UmbracoUserLogin>();

    public virtual ICollection<UmbracoUserStartNode> UmbracoUserStartNodes { get; set; } = new List<UmbracoUserStartNode>();

    public virtual ICollection<UmbracoUserGroup> UserGroups { get; set; } = new List<UmbracoUserGroup>();
}
