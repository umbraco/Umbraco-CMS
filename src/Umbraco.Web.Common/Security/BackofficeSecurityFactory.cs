using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Common.Security
{
    public class BackofficeSecurityFactory: IBackofficeSecurityFactory
    {
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IUserService _userService;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackofficeSecurityFactory(
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            IUserService userService,
            IGlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _userService = userService;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public void EnsureBackofficeSecurity()
        {
            if (_backofficeSecurityAccessor.BackofficeSecurity is null)
            {
                _backofficeSecurityAccessor.BackofficeSecurity = new BackofficeSecurity(_userService, _globalSettings, _hostingEnvironment, _httpContextAccessor);
            }

        }
    }
}
