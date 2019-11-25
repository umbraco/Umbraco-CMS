using System;
using System.Web.Security;
using Umbraco.Core.Composing;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.Membership
{
    internal static class MembershipUserExtensions
    {
        internal static UmbracoMembershipMember AsConcreteMembershipUser(this IMembershipUser member, string providerName, bool providerKeyAsGuid = false)
        {
            var membershipMember = new UmbracoMembershipMember(member, providerName, providerKeyAsGuid);
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
