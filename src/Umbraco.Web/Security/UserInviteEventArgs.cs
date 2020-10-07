using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Security
{
    public class UserInviteEventArgs : IdentityAuditEventArgs
    {
        public UserInviteEventArgs(string ipAddress, int performingUser, UserInvite invitedUser, IUser localUser, string comment = null)
            : base(AuditEvent.SendingUserInvite, ipAddress, comment, performingUser)
        {
            InvitedUser = invitedUser ?? throw new System.ArgumentNullException(nameof(invitedUser));
            User = localUser;
        }

        /// <summary>
        /// The model used to invite the user
        /// </summary>
        public UserInvite InvitedUser { get; }

        /// <summary>
        /// If event handler sets this to true it indicates that Umbraco will no try to send the invite itself
        /// </summary>
        public bool InviteHandled { get; set; }

        /// <summary>
        /// The local user that has been created that is pending the invite
        /// </summary>        
        public IUser User { get; }

        /// <summary>
        /// if set to true will show the edit user button in the UI, else it will not be shown
        /// </summary>
        public bool ShowUserResult { get; set; }

    }
}
