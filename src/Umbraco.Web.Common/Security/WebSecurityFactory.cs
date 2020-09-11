using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Common.Security
{
    public class WebSecurityFactory: IWebSecurityFactory
    {
        private readonly IWebSecurityAccessor _webSecurityAccessor;
        private readonly IUserService _userService;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebSecurityFactory(
            IWebSecurityAccessor webSecurityAccessor,
            IUserService userService,
            IGlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _webSecurityAccessor = webSecurityAccessor;
            _userService = userService;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public void EnsureWebSecurity()
        {
            if (_webSecurityAccessor.WebSecurity is null)
            {
                _webSecurityAccessor.WebSecurity = new WebSecurity(_userService, _globalSettings, _hostingEnvironment, _httpContextAccessor);
            }

        }
    }
}
