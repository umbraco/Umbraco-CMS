namespace Umbraco.Cms.Core.Models;

public class PublicAccessEntrySlim
{
    public Guid ContentId { get; set; }

    public string[] MemberUserNames { get; set; } = Array.Empty<string>();

    public string[] MemberGroupNames { get; set; } = Array.Empty<string>();

    public Guid LoginPageId { get; set; }

    public Guid ErrorPageId { get; set; }
}
