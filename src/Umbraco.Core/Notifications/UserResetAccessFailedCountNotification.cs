namespace Umbraco.Cms.Core.Notifications;

public class UserResetAccessFailedCountNotification : UserNotification
{
    public UserResetAccessFailedCountNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
