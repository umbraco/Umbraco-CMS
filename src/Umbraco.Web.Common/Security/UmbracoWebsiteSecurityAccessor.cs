using Microsoft.AspNetCore.Http;
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
        {
            get => _httpContextAccessor.HttpContext?.Features.Get<IUmbracoWebsiteSecurity>();
            set => _httpContextAccessor.HttpContext?.Features.Set(value);
        }
    }
}
