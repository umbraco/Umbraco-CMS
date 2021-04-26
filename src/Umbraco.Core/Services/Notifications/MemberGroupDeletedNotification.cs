using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class MemberGroupDeletedNotification : DeletedNotification<IMemberGroup>
    {
        public MemberGroupDeletedNotification(IMemberGroup target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
