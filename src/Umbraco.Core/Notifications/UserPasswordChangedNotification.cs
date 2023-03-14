namespace Umbraco.Cms.Core.Notifications;

public class UserPasswordChangedNotification : UserNotification
{
    public UserPasswordChangedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(
        ipAddress, affectedUserId, performingUserId)
    {
    }
}
