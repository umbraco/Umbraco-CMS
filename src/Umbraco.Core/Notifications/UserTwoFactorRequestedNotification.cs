namespace Umbraco.Cms.Core.Notifications;

public class UserTwoFactorRequestedNotification : INotification
{
    public UserTwoFactorRequestedNotification(Guid userKey) => UserKey = userKey;

    public Guid UserKey { get; }
}
