namespace Umbraco.Cms.Core.Services.Notifications
{
    public class RemovedMemberRolesNotification : MemberRolesNotification
    {
        public RemovedMemberRolesNotification(int[] memberIds, string[] roles) : base(memberIds, roles)
        {

        }
    }
}
