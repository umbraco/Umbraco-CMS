namespace Umbraco.Cms.Core.Notifications;

public class AssignedMemberRolesNotification : MemberRolesNotification
{
    public AssignedMemberRolesNotification(int[] memberIds, string[] roles)
        : base(memberIds, roles)
    {
    }
}
