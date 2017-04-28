using System;
using System.Web;
using Umbraco.Core.Security;

namespace Umbraco.Core.Auditing
{
    public class IdentityAuditEventArgs : EventArgs
    {
        public AuditEvent Action { get; set; }
        public DateTime DateTimeUtc { get; private set; }
        public string IpAddress { get; set; }
        public int AffectedUser { get; set; }
        public int PerformingUser { get; set; }
        public string Comment { get; set; }

        public IdentityAuditEventArgs(AuditEvent action, string ipAddress = "", int performingUser = -1)
        {
            DateTimeUtc = DateTime.UtcNow;
            Action = action;

            IpAddress = string.IsNullOrWhiteSpace(ipAddress)
                ? GetCurrentRequestIpAddress()
                : ipAddress;

            PerformingUser = performingUser == -1
                ? GetCurrentRequestBackofficeUserId()
                : performingUser;
        }

        /// <summary>
        /// Returns the current request IP address for logging if there is one
        /// </summary>
        /// <returns></returns>
        protected string GetCurrentRequestIpAddress()
        {
            var httpContext = HttpContext.Current == null ? (HttpContextBase)null : new HttpContextWrapper(HttpContext.Current);
            return httpContext.GetCurrentRequestIpAddress();
        }

        /// <summary>
        /// Returns the current request IP address for logging if there is one
        /// </summary>
        /// <returns></returns>
        protected int GetCurrentRequestBackofficeUserId()
        {
            if (HttpContext.Current == null) return 0;

            var authenticationTicket = new HttpContextWrapper(HttpContext.Current).GetUmbracoAuthTicket();
            if (authenticationTicket != null)
            {
                var currentBackofficeUser = ApplicationContext.Current.Services.UserService.GetByUsername(authenticationTicket.Name);
                if (currentBackofficeUser != null)
                    return currentBackofficeUser.Id;
            }

            // couldn't get auth ticket or current user, return default user (0)
            return 0;
        }
    }

    public enum AuditEvent
    {
        AccountLocked,
        AccountUnlocked,
        LoginSucces,
        Logout,
        AccessFailed,
        PasswordChanged,
        AccountCreated,
        ResetAccessFailedCount,
        AccountUpdated
    }
}
