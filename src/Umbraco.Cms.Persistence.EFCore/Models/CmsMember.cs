namespace Umbraco.Cms.Persistence.EFCore.Models;

public class CmsMember
{
    public int NodeId { get; set; }

    public string Email { get; set; } = null!;

    public string LoginName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? PasswordConfig { get; set; }

    public string? SecurityStampToken { get; set; }

    public DateTime? EmailConfirmedDate { get; set; }

    public int? FailedPasswordAttempts { get; set; }

    public bool? IsLockedOut { get; set; }

    public bool? IsApproved { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime? LastLockoutDate { get; set; }

    public DateTime? LastPasswordChangeDate { get; set; }

    public virtual UmbracoContent Node { get; set; } = null!;

    public virtual ICollection<UmbracoNode> MemberGroups { get; set; } = new List<UmbracoNode>();
}
