using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public abstract class MemberRolesNotification : INotification
    {
        protected MemberRolesNotification(int[] memberIds, string[] roles)
        {
            MemberIds = memberIds;
            Roles = roles;
        }

        public int[] MemberIds { get; }

        public string[] Roles { get; }
    }
}
