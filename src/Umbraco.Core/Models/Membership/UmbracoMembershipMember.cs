using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{
    internal class UmbracoMembershipMember : MembershipUser
    {
        private readonly IMember _member;

        public UmbracoMembershipMember(IMember member)
        {
            _member = member;
        }

        public override string Email
        {
            get { return _member.Email; }
            set { _member.Email = value; }
        }
    }
}