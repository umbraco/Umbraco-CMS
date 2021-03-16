namespace Umbraco.Cms.Web.Common.Security
{
    public class UserPasswordResetNotification : UserNotification
    {
        public UserPasswordResetNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
