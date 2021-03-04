using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Security
{
    // TODO: This is only for the back office, does it need to be in common?  YES currently UmbracoContext has an transitive dependency on this which needs to be fixed/reviewed.

    public class BackOfficeSecurityFactory: IBackOfficeSecurityFactory
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackOfficeSecurityFactory(
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
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
