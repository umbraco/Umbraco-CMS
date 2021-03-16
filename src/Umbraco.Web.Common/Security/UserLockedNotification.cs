namespace Umbraco.Cms.Web.Common.Security
{
    public class UserLockedNotification : UserNotification
    {
        public UserLockedNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
