using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// A <see cref="UserClaimsPrincipalFactory{TUser}" for the back office/>
    /// </summary>
    public class BackOfficeClaimsPrincipalFactory : UserClaimsPrincipalFactory<BackOfficeIdentityUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeClaimsPrincipalFactory"/> class.
        /// </summary>
        /// <param name="userManager">The user manager</param>
        /// <param name="optionsAccessor">The <see cref="BackOfficeIdentityOptions"/></param>
        public BackOfficeClaimsPrincipalFactory(UserManager<BackOfficeIdentityUser> userManager, IOptions<BackOfficeIdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// Returns a ClaimsIdentity that has the required claims, and allows flowing of claims from external identity
        /// </remarks>
        public override async Task<ClaimsPrincipal> CreateAsync(BackOfficeIdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            ClaimsIdentity baseIdentity = await base.GenerateClaimsAsync(user);

            baseIdentity.AddRequiredClaims(
                user.Id,
                user.UserName,
                user.Name,
                user.CalculatedContentStartNodeIds,
                user.CalculatedMediaStartNodeIds,
                user.Culture,
                user.SecurityStamp,
                user.AllowedSections,
                user.Roles.Select(x => x.RoleId).ToArray());

            // now we can flow any custom claims that the actual user has currently
            // assigned which could be done in the OnExternalLogin callback
            baseIdentity.MergeClaimsFromBackOfficeIdentity(user);

            return new ClaimsPrincipal(baseIdentity);
        }

        /// <inheritdoc />
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BackOfficeIdentityUser user)
        {
            // TODO: Have a look at the base implementation https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserClaimsPrincipalFactory.cs#L79
            // since it's setting an authentication type that is probably not what we want.
            // also, this is the method that we should be returning our UmbracoBackOfficeIdentity from , not the method above,
            // the method above just returns a principal that wraps the identity and we dont use a custom principal,
            // see https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserClaimsPrincipalFactory.cs#L66

            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);

            return identity;
        }
    }
}
