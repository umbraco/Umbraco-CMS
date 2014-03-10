﻿using System;
using System.Web.Security;
using Umbraco.Core.Services;

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

        private static MembershipScenario? _scenario = null;
        /// <summary>
        /// Returns the currently configured membership scenario for members in umbraco
        /// </summary>
        /// <value></value>
        internal static MembershipScenario GetMembershipScenario(this IMemberService memberService)
        {
            if (_scenario.HasValue == false)
            {
                if (System.Web.Security.Membership.Provider.Name == Constants.Conventions.Member.UmbracoMemberProviderName)
                {
                    return MembershipScenario.NativeUmbraco;
                }
                var memberType = ApplicationContext.Current.Services.MemberTypeService.Get(Constants.Conventions.MemberTypes.DefaultAlias);
                return memberType != null
                           ? MembershipScenario.CustomProviderWithUmbracoLink
                           : MembershipScenario.StandaloneCustomProvider;
            }
            return _scenario.Value;
        }
    }
}