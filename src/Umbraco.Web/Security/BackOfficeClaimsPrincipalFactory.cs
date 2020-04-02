using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Core.Security;
using Umbraco.Web.Models.Identity;

namespace Umbraco.Web.Security
{
    public class BackOfficeClaimsPrincipalFactory<TUser> : UserClaimsPrincipalFactory<TUser>
        where TUser : BackOfficeIdentityUser
    {
        public BackOfficeClaimsPrincipalFactory(UserManager<TUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            var claimsPrincipal = await base.CreateAsync(user);

            var umbracoIdentity = new UmbracoBackOfficeIdentity(
                claimsPrincipal.Identity as ClaimsIdentity,
                user.Id,
                user.UserName,
                user.Name,
                user.CalculatedContentStartNodeIds,
                user.CalculatedMediaStartNodeIds,
                user.Culture,
                //NOTE - there is no session id assigned here, this is just creating the identity, a session id will be generated when the cookie is written
                Guid.NewGuid().ToString(),
                user.SecurityStamp,
                user.AllowedSections,
                user.Roles.Select(x => x.RoleId).ToArray());

            return new ClaimsPrincipal(umbracoIdentity);
        }
    }
}
