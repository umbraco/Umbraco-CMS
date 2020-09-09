using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Security
{
    public class BackOfficeClaimsIdentityFactory<T> : ClaimsIdentityFactory<T, int>
        where T: BackOfficeIdentityUser
    {
        public BackOfficeClaimsIdentityFactory()
        {
            SecurityStampClaimType = Constants.Security.SessionIdClaimType;
            UserNameClaimType = ClaimTypes.Name;
        }

        /// <summary>
        /// Create a ClaimsIdentity from a user
        /// </summary>
        /// <param name="manager"/><param name="user"/><param name="authenticationType"/>
        /// <returns/>
        public override async Task<ClaimsIdentity> CreateAsync(UserManager<T, int> manager, T user, string authenticationType)
        {
            // TODO: This does not automatically apply claims from the User to the identity/ticket
            // So how can we flow Claims from the external identity to this one?
            // And can we do that without modifying the core? Can we replace the BackOfficeClaimsIdentityFactory easily? I don't actually think so...
            // we would need to replace the whole user manager to do that... can that be done in v7?
            // It could certainly be possible to just flow the Claims attached to user T to this identity
            // Another hack would be to modify the user manager to "SupportsUserClaim" and have an in-memory store of user claims for the user id
            // which would automatically be added with the base.CreateAsync.
            // Another way would be to persist the extra claims with the OnExternalLogin call into the extra storage for the user
            // and then implement SupportsUserClaim to extract the data from that extra storage. Not sure how backwards compat that is.

            var baseIdentity = await base.CreateAsync(manager, user, authenticationType);

            // now we can flow any custom claims that the actual user has currently assigned which could be done in the OnExternalLogin callback
            foreach (var claim in user.Claims)
            {
                baseIdentity.AddClaim(new Claim(claim.ClaimType, claim.ClaimValue));
            }

            var umbracoIdentity = new UmbracoBackOfficeIdentity(baseIdentity,
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

            return umbracoIdentity;
        }
    }

    public class BackOfficeClaimsIdentityFactory : BackOfficeClaimsIdentityFactory<BackOfficeIdentityUser>
    { }
}
