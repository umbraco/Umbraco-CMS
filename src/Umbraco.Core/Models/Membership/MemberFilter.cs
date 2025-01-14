namespace Umbraco.Cms.Core.Models.Membership;

public class MemberFilter
{
    public Guid? MemberTypeId { get; set; }

    public string? MemberGroupName { get; set; }

    public bool? IsApproved { get; set; }

    public bool? IsLockedOut { get; set; }

    public string? Filter { get; set; }
}
