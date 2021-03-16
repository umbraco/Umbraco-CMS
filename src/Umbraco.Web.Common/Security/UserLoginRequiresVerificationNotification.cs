namespace Umbraco.Cms.Web.Common.Security
{
    public class UserLoginRequiresVerificationNotification : UserNotification
    {
        public UserLoginRequiresVerificationNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
