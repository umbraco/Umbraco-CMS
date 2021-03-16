namespace Umbraco.Cms.Web.Common.Security
{
    public class UserLoginFailedNotification : UserNotification
    {
        public UserLoginFailedNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
