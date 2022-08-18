namespace Umbraco.Cms.Core.Notifications;

public class UserLoginSuccessNotification : UserNotification
{
    public UserLoginSuccessNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
