using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Common.Security
{
    public class BackOfficeSecurityFactory: IBackOfficeSecurityFactory
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IUserService _userService;
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackOfficeSecurityFactory(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IUserService userService,
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _backOfficeSecurityAccessor = backofficeSecurityAccessor;
            _userService = userService;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public void EnsureBackOfficeSecurity()
        {
            if (_backOfficeSecurityAccessor.BackOfficeSecurity is null)
            {
                _backOfficeSecurityAccessor.BackOfficeSecurity = new BackOfficeSecurity(_userService, _globalSettings, _hostingEnvironment, _httpContextAccessor);
            }

        }
    }
}
