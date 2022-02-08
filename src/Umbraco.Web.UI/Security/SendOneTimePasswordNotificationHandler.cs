using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.UI.Security
{
    public class SendOneTimePasswordNotificationHandler : INotificationHandler<MemberTwoFactorRequestedNotification>
    {
        public void Handle(MemberTwoFactorRequestedNotification notification)
        {

        }
    }
}
