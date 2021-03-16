namespace Umbraco.Cms.Web.Common.Security
{
    public class UserForgotPasswordChangedNotification : UserNotification
    {
        public UserForgotPasswordChangedNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }
    }
}
