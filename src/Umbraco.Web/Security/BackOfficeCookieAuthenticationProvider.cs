using Microsoft.Extensions.Options;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Services;

namespace Umbraco.Web.Security
{
    // TODO: Migrate this logic to cookie events in ConfigureUmbracoBackOfficeCookieOptions

    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
        private readonly IUserService _userService;
        private readonly IRuntimeState _runtimeState;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly SecuritySettings _securitySettings;

        public BackOfficeCookieAuthenticationProvider(
            IUserService userService,
            IRuntimeState runtimeState,
            IOptionsSnapshot<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IOptionsSnapshot<SecuritySettings> securitySettings)
        {
            _userService = userService;
            _runtimeState = runtimeState;
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _securitySettings = securitySettings.Value;
        }


        public override void ResponseSignOut(CookieResponseSignOutContext context)
        {            
        }
    }
}
