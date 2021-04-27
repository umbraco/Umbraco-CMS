using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class AssignedUserGroupPermissionsNotification : EnumerableObjectNotification<EntityPermission>
    {
        public AssignedUserGroupPermissionsNotification(IEnumerable<EntityPermission> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<EntityPermission> EntityPermissions => Target;
    }
}
