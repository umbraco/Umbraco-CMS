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
        private const string _identityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
        private const string _identityProviderClaimValue = "ASP.NET Identity";

        public BackOfficeClaimsPrincipalFactory(UserManager<TUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var baseIdentity = await base.GenerateClaimsAsync(user);

            // now we can flow any custom claims that the actual user has currently assigned which could be done in the OnExternalLogin callback
            foreach (var claim in user.Claims)
            {
                baseIdentity.AddClaim(new Claim(claim.ClaimType, claim.ClaimValue));
            }


            // Required by ASP.NET 4.x anti-forgery implementation
            baseIdentity.AddClaim(new Claim(_identityProviderClaimType, _identityProviderClaimValue));

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
