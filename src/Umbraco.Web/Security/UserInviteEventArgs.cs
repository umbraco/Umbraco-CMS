using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Security
{
    public class UserInviteEventArgs : IdentityAuditEventArgs
    {
        public UserInviteEventArgs(string ipAddress, int performingUser, UserInvite invitedUser, string comment = null)
            : base(AuditEvent.SendingUserInvite, ipAddress, comment, performingUser)
        {
            InvitedUser = invitedUser ?? throw new System.ArgumentNullException(nameof(invitedUser));
        }

        public UserInvite InvitedUser { get; }

        /// <summary>
        /// If event handler sets this to true it indicates that Umbraco will no try to send the invite itself
        /// </summary>
        public bool InviteHandled { get; set; }

        /// <summary>
        /// If the event handler has created a local user then this is the result which is used to return the details to the UI
        /// </summary>
        /// <remarks>
        /// It is optional to create a local user in this event. In many cases the custom invite flow will be for external logins and then local users will
        /// be created via the auto-linking process.
        /// </remarks>
        public IUser User { get; set; }        
    }
}
