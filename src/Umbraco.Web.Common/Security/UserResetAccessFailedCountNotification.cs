namespace Umbraco.Cms.Web.Common.Security
{
    public class UserResetAccessFailedCountNotification : UserNotification
    {
        public UserResetAccessFailedCountNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
