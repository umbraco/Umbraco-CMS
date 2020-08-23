using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{
    // TODO: Migrate this logic to cookie events in ConfigureUmbracoBackOfficeCookieOptions

    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
        private readonly IUserService _userService;
        private readonly IRuntimeState _runtimeState;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISecuritySettings _securitySettings;

        public BackOfficeCookieAuthenticationProvider(IUserService userService, IRuntimeState runtimeState, IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment, ISecuritySettings securitySettings)
        {
            _userService = userService;
            _runtimeState = runtimeState;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _securitySettings = securitySettings;
        }


        public override void ResponseSignOut(CookieResponseSignOutContext context)
        {
            
        }



    }
}
