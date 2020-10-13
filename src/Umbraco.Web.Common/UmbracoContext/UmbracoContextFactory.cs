using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Security;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
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
        private readonly IRequestAccessor _requestAccessor;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly UriUtility _uriUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoContextFactory"/> class.
        /// </summary>
        public UmbracoContextFactory(
            IUmbracoContextAccessor umbracoContextAccessor,
            IPublishedSnapshotService publishedSnapshotService,
            IVariationContextAccessor variationContextAccessor,
            IDefaultCultureAccessor defaultCultureAccessor,
            IOptions<GlobalSettings> globalSettings,
            IUserService userService,
            IHostingEnvironment hostingEnvironment,
            UriUtility uriUtility,
            IHttpContextAccessor httpContextAccessor,
            ICookieManager cookieManager,
            IRequestAccessor requestAccessor,
             IBackofficeSecurityAccessor backofficeSecurityAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _defaultCultureAccessor = defaultCultureAccessor ?? throw new ArgumentNullException(nameof(defaultCultureAccessor));
            _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _uriUtility = uriUtility ?? throw new ArgumentNullException(nameof(uriUtility));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
            _requestAccessor = requestAccessor ?? throw new ArgumentNullException(nameof(requestAccessor));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
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

            return new UmbracoContext(
                _publishedSnapshotService,
                _backofficeSecurityAccessor.BackofficeSecurity,
                _globalSettings,
                _hostingEnvironment,
                _variationContextAccessor,
                _uriUtility,
                _cookieManager,
                _requestAccessor);
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
