using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security
{
    public class UserInviteEventArgs : IdentityAuditEventArgs
    {
        public UserInviteEventArgs(string ipAddress, string performingUser, UserInvite invitedUser, IUser localUser, string comment = null)
            : base(AuditEvent.SendingUserInvite, ipAddress,  performingUser, comment, string.Intern(localUser.Id.ToString()), localUser.Name)
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
