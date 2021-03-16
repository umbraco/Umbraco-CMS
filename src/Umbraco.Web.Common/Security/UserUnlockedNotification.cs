namespace Umbraco.Cms.Web.Common.Security
{
    public class UserUnlockedNotification : UserNotification
    {
        public UserUnlockedNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
