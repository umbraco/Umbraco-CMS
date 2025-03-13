namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ProtectedAccess
{
    public static ProtectedAccess None => new(null, null);

    public ProtectedAccess(Guid? memberKey, string[]? memberRoles)
    {
        MemberKey = memberKey;
        MemberRoles = memberRoles;
    }

    public Guid? MemberKey { get; }

    public string[]? MemberRoles { get; }
}
