using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

public class UserPasswordResettingNotification :  CancelableObjectNotification<IUser>
{
    public UserPasswordResettingNotification(IUser target, EventMessages messages) : base(target, messages)
    {
    }

    public IUser User => Target;
}
