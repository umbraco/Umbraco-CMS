namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class AssignedMemberRolesNotification : MemberRolesNotification
    {
        public AssignedMemberRolesNotification(int[] memberIds, string[] roles) : base(memberIds, roles)
        {

        }
    }
}
