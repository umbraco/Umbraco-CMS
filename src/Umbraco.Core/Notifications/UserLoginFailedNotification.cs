namespace Umbraco.Cms.Core.Notifications;

public class UserLoginFailedNotification : UserNotification
{
    public UserLoginFailedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(
        ipAddress, affectedUserId, performingUserId)
    {
    }
}
