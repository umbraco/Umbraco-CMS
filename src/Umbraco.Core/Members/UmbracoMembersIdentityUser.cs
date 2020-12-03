using System;

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
        private int _id;

        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string MemberTypeAlias { get; set; }
        public bool IsLockedOut { get; set; }
        public string RawPasswordValue { get; set; }

        /// <summary>
        /// Returns true if an Id has been set on this object
        /// This will be false if the object is new and not persisted to the database
        /// </summary>
        public bool HasIdentity => _hasIdentity;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                _hasIdentity = true;
            }
        }

        //TODO: track
        public string PasswordHash { get; set; }

        //TODO: config
        public string PasswordConfig { get; set; }

        internal bool IsApproved;

        //TODO: needed?
        //public bool LoginsChanged;
        //public bool RolesChanged;


        public static UmbracoMembersIdentityUser CreateNew(
            string username,
            string email,
            string memberTypeAlias,
            string name)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));

            //no groups/roles yet
            var member = new UmbracoMembersIdentityUser
            {
                UserName = username,
                Email = email,
                Name = name,
                MemberTypeAlias = memberTypeAlias,
                Id = 0,  //TODO: is this meant to be 0 in this circumstance?
                //false by default unless specifically set
                _hasIdentity = false
            };

            //TODO: do we use this?
            //member.EnableChangeTracking();
            return member;
        }
    }
}
