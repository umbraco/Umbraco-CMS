using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security
{

    public class UmbracoWebsiteSecurityAccessor : IUmbracoWebsiteSecurityAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoWebsiteSecurityAccessor"/> class.
        /// </summary>
        public UmbracoWebsiteSecurityAccessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Gets or sets the <see cref="IUmbracoWebsiteSecurity"/> object.
        /// </summary>
        public IUmbracoWebsiteSecurity WebsiteSecurity
            => _httpContextAccessor.HttpContext?.RequestServices.GetService<IUmbracoWebsiteSecurity>();
    }
}
