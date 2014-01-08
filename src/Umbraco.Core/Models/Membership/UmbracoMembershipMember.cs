using System;
using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{

    internal class UmbracoMembershipMember : MembershipUser
    {
        private readonly IMembershipUser _member;
        private readonly string _userName;
        private readonly object _providerUserKey;
        private readonly string _passwordQuestion;
        private readonly bool _isLockedOut;
        private readonly DateTime _lastLockoutDate;
        private readonly DateTime _creationDate;
        private DateTime _lastLoginDate;
        private readonly DateTime _lastPasswordChangedDate;
        private readonly string _providerName;
        private string _email;
        private string _comment;
        private bool _isApproved;
        private DateTime _lastActivityDate;

        //NOTE: We are only overriding the properties that matter, we don't override things like IsOnline since that is handled with the sub-class and the membership providers.

        //NOTE: We are not calling the base constructor which will validate that a provider with the specified name exists which causes issues with unit tests. The ctor
        // validation for that doesn't need to be there anyways (have checked the source).
        public UmbracoMembershipMember(IMembershipUser member, string providerName)
        {
            _member = member;
            //NOTE: We are copying the values here so that everything is consistent with how the underlying built-in ASP.Net membership user
            // handles data! We don't want to do anything differently there but since we cannot use their ctor we'll need to handle this logic ourselves.
            if (member.Username != null)
                _userName = member.Username.Trim();
            if (member.Email != null)
                _email = member.Email.Trim();
            if (member.PasswordQuestion != null)
                _passwordQuestion = member.PasswordQuestion.Trim();
            _providerName = providerName;
            _providerUserKey = member.ProviderUserKey;
            _comment = member.Comments;
            _isApproved = member.IsApproved;
            _isLockedOut = member.IsLockedOut;
            _creationDate = member.CreateDate.ToUniversalTime();
            _lastLoginDate = member.LastLoginDate.ToUniversalTime();
            //TODO: We currently don't really have any place to store this data!!
            _lastActivityDate = member.LastLoginDate.ToUniversalTime();
            _lastPasswordChangedDate = member.LastPasswordChangeDate.ToUniversalTime();
            _lastLockoutDate = member.LastLockoutDate.ToUniversalTime();
        }
        
        internal IMembershipUser Member
        {
            get { return _member; }
        }

        public override string UserName
        {
            get { return _userName; }
        }

        public override object ProviderUserKey
        {
            get { return _providerUserKey; }
        }

        public override string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public override string PasswordQuestion
        {
            get { return _passwordQuestion; }
        }

        public override string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public override bool IsApproved
        {
            get { return _isApproved; }
            set { _isApproved = value; }
        }

        public override bool IsLockedOut
        {
            get { return _isLockedOut; }
        }

        public override DateTime LastLockoutDate
        {
            get { return _lastLockoutDate; }
        }

        public override DateTime CreationDate
        {
            get { return _creationDate; }
        }

        public override DateTime LastLoginDate
        {
            get { return _lastLoginDate; }
            set { _lastLoginDate = value; }
        }

        public override DateTime LastActivityDate
        {
            get { return _lastActivityDate; }
            set { _lastActivityDate = value; }
        }

        public override DateTime LastPasswordChangedDate
        {
            get { return _lastPasswordChangedDate; }
        }

        public override string ProviderName
        {
            get { return _providerName; }
        }
    }
}