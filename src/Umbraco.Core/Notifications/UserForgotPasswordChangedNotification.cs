namespace Umbraco.Cms.Core.Notifications;

public class UserForgotPasswordChangedNotification : UserNotification
{
    public UserForgotPasswordChangedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
