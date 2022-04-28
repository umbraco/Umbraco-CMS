using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security
{
    public class BackOfficeSecurityAccessor : IBackOfficeSecurityAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeSecurityAccessor"/> class.
        /// </summary>
        public BackOfficeSecurityAccessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Gets the current <see cref="IBackOfficeSecurity"/> object.
        /// </summary>
        public IBackOfficeSecurity? BackOfficeSecurity
            => _httpContextAccessor.HttpContext?.RequestServices?.GetService<IBackOfficeSecurity>();
    }
}
