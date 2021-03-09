using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Security
{

    public class HybridUmbracoWebsiteSecurityAccessor : HybridAccessorBase<IUmbracoWebsiteSecurity>, IUmbracoWebsiteSecurityAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HybridUmbracoWebsiteSecurityAccessor"/> class.
        /// </summary>
        public HybridUmbracoWebsiteSecurityAccessor(IRequestCache requestCache)
            : base(requestCache)
        { }

        /// <summary>
        /// Gets or sets the <see cref="IUmbracoWebsiteSecurity"/> object.
        /// </summary>
        public IUmbracoWebsiteSecurity WebsiteSecurity
        {
            get => Value;
            set => Value = value;
        }
    }
}
