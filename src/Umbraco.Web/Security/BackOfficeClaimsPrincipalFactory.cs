using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Core;
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
            var baseIdentity = await base.GenerateClaimsAsync(user);
            baseIdentity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity"));
            
            var umbracoIdentity = new UmbracoBackOfficeIdentity(
                baseIdentity,
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
