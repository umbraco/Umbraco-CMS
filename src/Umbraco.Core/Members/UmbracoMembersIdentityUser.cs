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
        private bool _hasIdentity;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string MemberTypeAlias { get; set; }
        public bool IsLockedOut { get; set; }

        public string RawPasswordValue { get; set; }
        public DateTime LastPasswordChangeDateUtc { get; set; }

        /// <summary>
        /// Returns true if an Id has been set on this object
        /// This will be false if the object is new and not persisted to the database
        /// </summary>
        public bool HasIdentity => _hasIdentity;

        //TODO: track
        public string PasswordHash { get; set; }

        //TODO: config
        public string PasswordConfig { get; set; }

        string Comment;
        internal bool IsApproved;
        DateTime LastLockoutDate;
        DateTime CreationDate;
        DateTime LastLoginDate;
        DateTime LastActivityDate;

        //TODO: needed?
        //public bool LoginsChanged;
        //public bool RolesChanged;


        public static UmbracoMembersIdentityUser CreateNew(string username, string email, string name = null)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));

            //no groups/roles yet
            var member = new UmbracoMembersIdentityUser
            {
                UserName = username,
                Email = email,
                Name = name,
                Id = 0,  //TODO
                _hasIdentity = false
            };

            //TODO: do we use this?
            //member.EnableChangeTracking();
            return member;
        }
    }
}
