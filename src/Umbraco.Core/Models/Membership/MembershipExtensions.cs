using System;
using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{
    internal static class MembershipExtensions
    {
        internal static MembershipUser AsConcreteMembershipUser(this IMember member)
        {
            var membershipMember = new UmbracoMembershipMember(member);
            return membershipMember;
        }

        internal static IMember AsIMember(this MembershipUser membershipMember)
        {
            var member = membershipMember as UmbracoMembershipMember;
            if (member != null)
            {
                return member.Member;
            }

            throw new NotImplementedException();
        }
    }
}