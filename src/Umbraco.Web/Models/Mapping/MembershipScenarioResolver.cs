using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.Mapping
{
    internal class MembershipScenarioResolver
    {
        private readonly IMemberTypeService _memberTypeService;

        public MembershipScenarioResolver(IMemberTypeService memberTypeService)
        {
            _memberTypeService = memberTypeService;
        }

        public MembershipScenario Resolve(IMember source)
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider())
            {
                return MembershipScenario.NativeUmbraco;
            }
            var memberType = _memberTypeService.Get(Constants.Conventions.MemberTypes.DefaultAlias);
            return memberType != null
                ? MembershipScenario.CustomProviderWithUmbracoLink
                : MembershipScenario.StandaloneCustomProvider;
        }
    }
}
