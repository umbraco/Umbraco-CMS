using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{
    //TODO: THere's still a bunch of properties that don't exist in this use that need to be mapped somehow. 

    internal class UmbracoMembershipMember : MembershipUser
    {
        private readonly IMember _member;

        public UmbracoMembershipMember(IMember member)
        {
            _member = member;
        }

        internal IMember Member
        {
            get { return _member; }
        }

        public override string Email
        {
            get { return _member.Email; }
            set { _member.Email = value; }
        }

        public override object ProviderUserKey
        {
            get { return _member.Key; }
        }

        public override System.DateTime CreationDate
        {
            get { return _member.CreateDate; }
        }

        public override string UserName
        {
            get { return _member.Username; }
        }

        public override string Comment
        {
            get { return _member.Comments; }
            set { _member.Comments = value; }
        }

        public override bool IsApproved
        {
            get { return _member.IsApproved; }
            set { _member.IsApproved = value; }
        }

        public override bool IsLockedOut
        {
            get { return _member.IsLockedOut; }
        }

        public override System.DateTime LastLoginDate
        {
            get { return _member.LastLoginDate; }
            set { _member.LastLoginDate = value; }
        }

    }
}