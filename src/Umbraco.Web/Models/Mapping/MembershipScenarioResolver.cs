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
        private readonly Lazy<IMemberTypeService> _memberTypeService;

        public MembershipScenarioResolver(Lazy<IMemberTypeService> memberTypeService)
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
            var memberType = _memberTypeService.Value.Get(Constants.Conventions.MemberTypes.DefaultAlias);
            return memberType != null
                ? MembershipScenario.CustomProviderWithUmbracoLink
                : MembershipScenario.StandaloneCustomProvider;
        }
    }
}
