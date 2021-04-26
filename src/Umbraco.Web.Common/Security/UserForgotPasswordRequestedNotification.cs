namespace Umbraco.Cms.Web.Common.Security
{
    public class UserForgotPasswordRequestedNotification : UserNotification
    {
        public UserForgotPasswordRequestedNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
