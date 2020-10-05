namespace Umbraco.Web.Security
{
    /// <summary>
    /// Event args used when signing out
    /// </summary>
    public class SignOutAuditEventArgs : IdentityAuditEventArgs
    {
        public SignOutAuditEventArgs(AuditEvent action, string ipAddress, string comment = null, int performingUser = -1, int affectedUser = -1)
            : base(action, ipAddress, comment, performingUser, affectedUser)
        {
        }

        /// <summary>
        /// Allows event handlers to set a GET absolute URL to be redirected to after successful logout out of the back office. This
        /// can be used for external login providers.
        /// </summary>
        public string SignOutRedirectUrl { get; set; }
    }
}
