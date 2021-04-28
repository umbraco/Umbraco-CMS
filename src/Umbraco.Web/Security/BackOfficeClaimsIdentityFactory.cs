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
            var baseIdentity = await base.CreateAsync(manager, user, authenticationType);

            var umbracoIdentity = new UmbracoBackOfficeIdentity(baseIdentity,
                user.Id,
                user.UserName,
                user.Name,
                user.CalculatedContentStartNodeIds,
                user.CalculatedMediaStartNodeIds,
                user.Culture,
                // NOTE - there is no session id assigned here, this is just creating the identity, a session id will be generated when the cookie is written
                Guid.NewGuid().ToString(),
                user.SecurityStamp,
                user.AllowedSections,
                user.Roles.Select(x => x.RoleId).ToArray());

            // now we can flow any custom claims that the actual user has currently
            // assigned which could be done in the OnExternalLogin callback
            umbracoIdentity.MergeClaimsFromBackOfficeIdentity(user);

            return umbracoIdentity;
        }
    }

    public class BackOfficeClaimsIdentityFactory : BackOfficeClaimsIdentityFactory<BackOfficeIdentityUser>
    { }
}
