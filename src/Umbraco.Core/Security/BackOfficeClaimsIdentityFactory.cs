using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    public class BackOfficeClaimsIdentityFactory<T> : ClaimsIdentityFactory<T, int>
        where T: BackOfficeIdentityUser
    {
        private readonly ApplicationContext _appCtx;

        [Obsolete("Use the overload specifying all dependencies instead")]
        public BackOfficeClaimsIdentityFactory()
            :this(ApplicationContext.Current)
        {
        }

        public BackOfficeClaimsIdentityFactory(ApplicationContext appCtx)
        {
            if (appCtx == null) throw new ArgumentNullException("appCtx");
            _appCtx = appCtx;

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
                //NOTE - there is no session id assigned here, this is just creating the identity, a session id will be generated when the cookie is written
                new UserData
                {
                    Id = user.Id,
                    Username = user.UserName,
                    RealName = user.Name,
                    AllowedApplications = user.AllowedSections,
                    Culture = user.Culture,                 
                    Roles = user.Roles.Select(x => x.RoleId).ToArray(),
                    StartContentNodes = user.CalculatedContentStartNodeIds,
                    StartMediaNodes = user.CalculatedMediaStartNodeIds,
                    SecurityStamp = user.SecurityStamp
                });

            return umbracoIdentity;
        }        
    }

    public class BackOfficeClaimsIdentityFactory : BackOfficeClaimsIdentityFactory<BackOfficeIdentityUser>
    {
        [Obsolete("Use the overload specifying all dependencies instead")]
        public BackOfficeClaimsIdentityFactory()
        {
        }

        public BackOfficeClaimsIdentityFactory(ApplicationContext appCtx) : base(appCtx)
        {
        }
    }
}