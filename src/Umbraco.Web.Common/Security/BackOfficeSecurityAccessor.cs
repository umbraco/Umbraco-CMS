using Microsoft.AspNetCore.Http;
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
        /// Gets or sets the <see cref="IBackOfficeSecurity"/> object.
        /// </summary>
        public IBackOfficeSecurity BackOfficeSecurity
        {
            get => _httpContextAccessor.HttpContext?.Features.Get<IBackOfficeSecurity>();
            set => _httpContextAccessor.HttpContext?.Features.Set(value);
        }
    }
}
