using System;
using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{
    internal static class MembershipExtensions
    {
        internal static UmbracoMembershipMember AsConcreteMembershipUser(this IMembershipUser member, string providerName)
        {
            var membershipMember = new UmbracoMembershipMember(member, providerName);
            return membershipMember;
        }

        internal static IMembershipUser AsIMember(this UmbracoMembershipMember membershipMember)
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