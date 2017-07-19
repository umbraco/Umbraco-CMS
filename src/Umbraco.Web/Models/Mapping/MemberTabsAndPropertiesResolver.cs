using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A custom tab/property resolver for members which will ensure that the built-in membership properties are or arent' displayed
    /// depending on if the member type has these properties
    /// </summary>
    /// <remarks>
    /// This also ensures that the IsLocked out property is readonly when the member is not locked out - this is because
    /// an admin cannot actually set isLockedOut = true, they can only unlock.
    /// </remarks>
    internal class MemberTabsAndPropertiesResolver : TabsAndPropertiesResolver
    {
        private readonly ILocalizedTextService _localizedTextService;

        public MemberTabsAndPropertiesResolver(ILocalizedTextService localizedTextService)
            : base(localizedTextService)
        {
            _localizedTextService = localizedTextService;
        }

        public MemberTabsAndPropertiesResolver(ILocalizedTextService localizedTextService,
            IEnumerable<string> ignoreProperties) : base(localizedTextService, ignoreProperties)
        {
            _localizedTextService = localizedTextService;
        }

        public override IEnumerable<Tab<ContentPropertyDisplay>> Resolve(IContentBase content)
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            IgnoreProperties = content.PropertyTypes
                .Where(x => x.HasIdentity == false)
                .Select(x => x.Alias)
                .ToArray();

            var result = base.Resolve(content).ToArray();

            if (provider.IsUmbracoMembershipProvider() == false)
            {
                //it's a generic provider so update the locked out property based on our known constant alias
                var isLockedOutProperty = result.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == Constants.Conventions.Member.IsLockedOut);
                if (isLockedOutProperty != null && isLockedOutProperty.Value.ToString() != "1")
                {
                    isLockedOutProperty.View = "readonlyvalue";
                    isLockedOutProperty.Value = _localizedTextService.Localize("general/no");
                }

                return result;
            }
            else
            {
                var umbracoProvider = (IUmbracoMemberTypeMembershipProvider) provider;

                //This is kind of a hack because a developer is supposed to be allowed to set their property editor - would have been much easier
                // if we just had all of the membeship provider fields on the member table :(
                // TODO: But is there a way to map the IMember.IsLockedOut to the property ? i dunno.
                var isLockedOutProperty = result.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == umbracoProvider.LockPropertyTypeAlias);
                if (isLockedOutProperty != null && isLockedOutProperty.Value.ToString() != "1")
                {
                    isLockedOutProperty.View = "readonlyvalue";
                    isLockedOutProperty.Value = _localizedTextService.Localize("general/no");
                }

                return result;
            }
        }
    }
}