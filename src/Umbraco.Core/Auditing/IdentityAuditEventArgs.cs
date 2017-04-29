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
            var owinContext = HttpContext.Current.GetOwinContext();
            return owinContext == null ? string.Empty : owinContext.Request.RemoteIpAddress;
        }

        /// <summary>
        /// Returns the current logged in backoffice user's Id logging if there is one
        /// </summary>
        /// <returns></returns>
        protected int GetCurrentRequestBackofficeUserId()
        {
            var userId = 0;
            if (HttpContext.Current == null) return userId;
            var owinContext = HttpContext.Current.GetOwinContext();
            if (owinContext == null) return userId;
            var backOfficeIdentity = owinContext.Authentication.User.GetUmbracoIdentity();
            if (backOfficeIdentity != null)
                int.TryParse(backOfficeIdentity.Id.ToString(), out userId);
            return userId;
        }
    }

    public enum AuditEvent
    {
        AccountLocked,
        AccountUnlocked,
        LoginSucces,
        LogoutSuccess,
        AccessFailed,
        PasswordChanged,
        AccountCreated, //not yet being called
        ResetAccessFailedCount,
        AccountUpdated,
        PasswordReset
    }
}
