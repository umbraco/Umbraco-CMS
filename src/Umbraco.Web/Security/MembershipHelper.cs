using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Umbraco.Web.Security
{
    internal class MembershipHelper
    {

        public MembershipUser UpdateMember(MembershipUser member, MembershipProvider provider,
            string email = null,
            bool? isApproved = null,
            bool? isLocked = null,
            DateTime? lastLoginDate = null,
            DateTime? lastActivityDate = null,
            string comment = null)
        {
            //set the writable properties
            if (email != null)
            {
                member.Email = email;    
            }
            if (isApproved.HasValue)
            {
                member.IsApproved = isApproved.Value;    
            }
            if (lastLoginDate.HasValue)
            {
                member.LastLoginDate = lastLoginDate.Value;
            }
            if (lastActivityDate.HasValue)
            {
                member.LastActivityDate = lastActivityDate.Value;
            }
            if (comment != null)
            {
                member.Comment = comment;
            }

            if (isLocked.HasValue)
            {
                //there is no 'setter' on IsLockedOut but you can ctor a new membership user with it set, so i guess that's what we'll do,
                // this does mean however if it was a typed membership user object that it will no longer be typed
                //membershipUser.IsLockedOut = true;
                member = new MembershipUser(member.ProviderName, member.UserName, 
                    member.ProviderUserKey, member.Email, member.PasswordQuestion, member.Comment, member.IsApproved, 
                    isLocked.Value,  //new value
                    member.CreationDate, member.LastLoginDate, member.LastActivityDate, member.LastPasswordChangedDate, member.LastLockoutDate);
            }

            provider.UpdateUser(member);

            return member;
        }

    }
}
