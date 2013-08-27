using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{
    internal class UmbracoMember : MembershipUser
    {
        private readonly IMembershipUser _member;

        public UmbracoMember(IMembershipUser member)
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