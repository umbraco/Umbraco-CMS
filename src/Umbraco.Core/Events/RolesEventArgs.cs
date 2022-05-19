namespace Umbraco.Cms.Core.Events;

public class RolesEventArgs : EventArgs
{
    public RolesEventArgs(int[] memberIds, string[] roles)
    {
        MemberIds = memberIds;
        Roles = roles;
    }

    public int[] MemberIds { get; set; }

    public string[] Roles { get; set; }
}
