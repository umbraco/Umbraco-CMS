using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;


namespace Umbraco.Cms.Web.Common.Security
{
    public class MemberClaimsPrincipalFactory : UserClaimsPrincipalFactory<MemberIdentityUser>
    {
        public MemberClaimsPrincipalFactory(UserManager<MemberIdentityUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }
    }
}
