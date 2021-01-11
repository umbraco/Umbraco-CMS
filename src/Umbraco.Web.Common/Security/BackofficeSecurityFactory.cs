using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Common.Security
{
    // TODO: This is only for the back office, does it need to be in common?

    public class BackOfficeSecurityFactory: IBackOfficeSecurityFactory
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackOfficeSecurityFactory(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _backOfficeSecurityAccessor = backofficeSecurityAccessor;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public void EnsureBackOfficeSecurity()
        {
            if (_backOfficeSecurityAccessor.BackOfficeSecurity is null)
            {
                _backOfficeSecurityAccessor.BackOfficeSecurity = new BackOfficeSecurity(_userService,  _httpContextAccessor);
            }

        }
    }
}
