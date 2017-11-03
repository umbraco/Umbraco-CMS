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

            //create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one
            
            var session = _appCtx.IsConfigured && _appCtx.IsUpgrading == false
                ? _appCtx.Services.UserService.CreateLoginSession(user.Id, GetCurrentRequestIpAddress())
                : Guid.NewGuid();

            var umbracoIdentity = new UmbracoBackOfficeIdentity(baseIdentity,
                //set a new session id
                new UserData(session.ToString())
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

        /// <summary>
        /// Returns the current request IP address for logging if there is one
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCurrentRequestIpAddress()
        {
            //TODO: inject a service to get this value, we should not be relying on the old HttpContext.Current especially in the ASP.NET Identity world - though it's difficult to get a reference to the request from the usermanager since the request holds a reference to the usermanager. Anyways, not time right now.
            var httpContext = HttpContext.Current == null ? (HttpContextBase)null : new HttpContextWrapper(HttpContext.Current);
            return httpContext.GetCurrentRequestIpAddress();
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