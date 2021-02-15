using Umbraco.Cms.Core.Security;
using Umbraco.Web.Security;

namespace Umbraco.Web.Macros
{
    internal class MemberUserKeyProvider : IMemberUserKeyProvider
    {
        public object GetMemberProviderUserKey()
        {
            //ugh, membershipproviders :(
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            var member = MembershipProviderExtensions.GetCurrentUser(provider);

            return member?.ProviderUserKey;
        }
    }
}
