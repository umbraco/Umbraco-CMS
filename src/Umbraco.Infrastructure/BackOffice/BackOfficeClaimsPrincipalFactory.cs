using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Umbraco.Core.BackOffice
{
    public class BackOfficeClaimsPrincipalFactory<TUser> : UserClaimsPrincipalFactory<TUser>
        where TUser : BackOfficeIdentityUser
    {
        public BackOfficeClaimsPrincipalFactory(UserManager<TUser> userManager, IOptions<BackOfficeIdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var baseIdentity = await base.GenerateClaimsAsync(user);

            var umbracoIdentity = new UmbracoBackOfficeIdentity(
                baseIdentity,
                user.Id,
                user.UserName,
                user.Name,
                user.CalculatedContentStartNodeIds,
                user.CalculatedMediaStartNodeIds,
                user.Culture,
                user.SecurityStamp,
                user.AllowedSections,
                user.Roles.Select(x => x.RoleId).ToArray());

            return new ClaimsPrincipal(umbracoIdentity);
        }
    }
}
