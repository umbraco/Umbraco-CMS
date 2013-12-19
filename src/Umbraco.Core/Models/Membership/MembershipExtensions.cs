using System;
using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{
    internal static class MembershipExtensions
    {
        internal static UmbracoMembershipMember AsConcreteMembershipUser(this IMember member)
        {
            var membershipMember = new UmbracoMembershipMember(member);
            return membershipMember;
        }

        internal static IMember AsIMember(this UmbracoMembershipMember membershipMember)
        {
            var member = membershipMember;
            if (member != null)
            {
                return member.Member;
            }

            throw new NotImplementedException();
        }
    }
}