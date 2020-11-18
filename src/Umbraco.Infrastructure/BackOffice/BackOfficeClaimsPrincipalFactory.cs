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

            // now we can flow any custom claims that the actual user has currently assigned which could be done in the OnExternalLogin callback
            foreach (var claim in user.Claims)
            {
                baseIdentity.AddClaim(new Claim(claim.ClaimType, claim.ClaimValue));
            }


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

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user)
        {
            // TODO: Have a look at the base implementation https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserClaimsPrincipalFactory.cs#L79
            // since it's setting an authentication type that is probably not what we want.
            // also, this is the method that we should be returning our UmbracoBackOfficeIdentity from , not the method above,
            // the method above just returns a principal that wraps the identity and we dont use a custom principal,
            // see https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserClaimsPrincipalFactory.cs#L66

            var identity = await base.GenerateClaimsAsync(user);

            return identity;
        }
    }
}
