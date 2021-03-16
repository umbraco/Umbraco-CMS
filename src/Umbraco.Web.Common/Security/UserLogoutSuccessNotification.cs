namespace Umbraco.Cms.Web.Common.Security
{
    public class UserLogoutSuccessNotification : UserNotification
    {
        public UserLogoutSuccessNotification(string ipAddress, string affectedUserId, string performingUserId) : base(ipAddress, affectedUserId, performingUserId)
        {
        }

        public string SignOutRedirectUrl { get; set; }
    }
}
