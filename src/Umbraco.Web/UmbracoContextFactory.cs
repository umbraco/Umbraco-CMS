using System;
using System.IO;
using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    // NOTE: This has been migrated to netcore

    /// <summary>
    /// Creates and manages <see cref="IUmbracoContext"/> instances.
    /// </summary>
    public class UmbracoContextFactory : IUmbracoContextFactory
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;

        private readonly GlobalSettings _globalSettings;
        private readonly IUserService _userService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICookieManager _cookieManager;
        private readonly UriUtility _uriUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoContextFactory"/> class.
        /// </summary>
        public UmbracoContextFactory(
            IUmbracoContextAccessor umbracoContextAccessor,
            IPublishedSnapshotService publishedSnapshotService,
            IVariationContextAccessor variationContextAccessor,
            IDefaultCultureAccessor defaultCultureAccessor,
            GlobalSettings globalSettings,
            IUserService userService,
            IHostingEnvironment hostingEnvironment,
            UriUtility uriUtility,
            IHttpContextAccessor httpContextAccessor,
            ICookieManager cookieManager)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _defaultCultureAccessor = defaultCultureAccessor ?? throw new ArgumentNullException(nameof(defaultCultureAccessor));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _hostingEnvironment = hostingEnvironment;
            _uriUtility = uriUtility;
            _httpContextAccessor = httpContextAccessor;
            _cookieManager = cookieManager;
        }

        private IUmbracoContext CreateUmbracoContext()
        {
            // make sure we have a variation context
            if (_variationContextAccessor.VariationContext == null)
            {
                // TODO: By using _defaultCultureAccessor.DefaultCulture this means that the VariationContext will always return a variant culture, it will never
                // return an empty string signifying that the culture is invariant. But does this matter? Are we actually expecting this to return an empty string
                // for invariant routes? From what i can tell throughout the codebase is that whenever we are checking against the VariationContext.Culture we are
                // also checking if the content type varies by culture or not. This is fine, however the code in the ctor of VariationContext is then misleading
                // since it's assuming that the Culture can be empty (invariant) when in reality of a website this will never be empty since a real culture is always set here.
                _variationContextAccessor.VariationContext = new VariationContext(_defaultCultureAccessor.DefaultCulture);
            }

            return new UmbracoContext(_httpContextAccessor, _publishedSnapshotService, new BackOfficeSecurity(), _globalSettings, _hostingEnvironment, _variationContextAccessor, _uriUtility, _cookieManager);
        }

        /// <inheritdoc />
        public UmbracoContextReference EnsureUmbracoContext()
        {
            var currentUmbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (currentUmbracoContext != null)
                return new UmbracoContextReference(currentUmbracoContext, false, _umbracoContextAccessor);


            var umbracoContext = CreateUmbracoContext();
            _umbracoContextAccessor.UmbracoContext = umbracoContext;

            return new UmbracoContextReference(umbracoContext, true, _umbracoContextAccessor);
        }
    }
}
