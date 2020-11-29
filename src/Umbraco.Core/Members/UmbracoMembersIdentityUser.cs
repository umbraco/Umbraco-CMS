using System;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Members
{
    /// <summary>
    /// An Umbraco member user type
    /// </summary>
    public class UmbracoMembersIdentityUser
    //: IRememberBeingDirty
    //TODO: use of identity classes
    //: IdentityUser<int, IIdentityUserLogin, IdentityUserRole<string>, IdentityUserClaim<int>>, 
    {
        public int Id;
        public string Name;
        public string Email;
        public string UserName;
        public string MemberTypeAlias;
        public bool IsLockedOut;

        string Comment;
        bool IsApproved;
        DateTime LastLockoutDate;
        DateTime CreationDate;
        DateTime LastLoginDate;
        DateTime LastActivityDate;
        DateTime LastPasswordChangedDate;

        //TODO: needed?
        //public bool LoginsChanged;
        //public bool RolesChanged;
    }
}
