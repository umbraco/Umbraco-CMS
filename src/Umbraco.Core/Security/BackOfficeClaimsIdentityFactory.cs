using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
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
                //set a new session id
                new UserData
                {
                    Id = user.Id,
                    Username = user.UserName,
                    RealName = user.Name,
                    AllowedApplications = user.AllowedSections,
                    Culture = user.Culture,
                    //TODO: In order for this to work, the user.Roles would need to be filled in! 
                    //Currently that is not the case because the BackOfficeIdentityUser deals with Groups (which we need to update)
                    //For now, I'll fix this by using the user.Groups instead
                    //Roles = user.Roles.Select(x => x.RoleId).ToArray(),
                    Roles = user.Groups.Select(x => x.Alias).ToArray(),
                    StartContentNodes = user.AllStartContentIds,
                    StartMediaNodes = user.AllStartMediaIds,
                    SessionId = user.SecurityStamp
                });

            return umbracoIdentity;
        }
    }

    public class BackOfficeClaimsIdentityFactory : BackOfficeClaimsIdentityFactory<BackOfficeIdentityUser>
    {       

    }
}