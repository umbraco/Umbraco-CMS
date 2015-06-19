using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class BackOfficeClaimsIdentityFactory : ClaimsIdentityFactory<BackOfficeIdentityUser, int>
    {
        /// <summary>
        /// Create a ClaimsIdentity from a user
        /// </summary>
        /// <param name="manager"/><param name="user"/><param name="authenticationType"/>
        /// <returns/>
        public override async Task<ClaimsIdentity> CreateAsync(UserManager<BackOfficeIdentityUser, int> manager, BackOfficeIdentityUser user, string authenticationType)
        {
            var baseIdentity = await base.CreateAsync(manager, user, authenticationType);

            var umbracoIdentity = new UmbracoBackOfficeIdentity(baseIdentity,
                //set a new session id
                new UserData(Guid.NewGuid().ToString("N"))
                {
                    Id = user.Id,
                    Username = user.UserName,
                    RealName = user.Name,
                    AllowedApplications = user.AllowedSections,
                    Culture = user.Culture,
                    Roles = user.Roles.Select(x => x.RoleId).ToArray(),
                    StartContentNode = user.StartContentId,
                    StartMediaNode = user.StartMediaId
                });

            return umbracoIdentity;
        }
    }
}