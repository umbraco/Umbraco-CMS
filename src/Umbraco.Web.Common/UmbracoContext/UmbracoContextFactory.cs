using System;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Routing;
using Umbraco.Core.Security;
using Umbraco.Web.PublishedCache;

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

        private readonly UmbracoRequestPaths _umbracoRequestPaths;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICookieManager _cookieManager;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly UriUtility _uriUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoContextFactory"/> class.
        /// </summary>
        public UmbracoContextFactory(
            IUmbracoContextAccessor umbracoContextAccessor,
            IPublishedSnapshotService publishedSnapshotService,
            IVariationContextAccessor variationContextAccessor,
            IDefaultCultureAccessor defaultCultureAccessor,
            UmbracoRequestPaths umbracoRequestPaths,
            IHostingEnvironment hostingEnvironment,
            UriUtility uriUtility,
            ICookieManager cookieManager,
            IRequestAccessor requestAccessor,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _defaultCultureAccessor = defaultCultureAccessor ?? throw new ArgumentNullException(nameof(defaultCultureAccessor));
            _umbracoRequestPaths = umbracoRequestPaths ?? throw new ArgumentNullException(nameof(umbracoRequestPaths));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _uriUtility = uriUtility ?? throw new ArgumentNullException(nameof(uriUtility));
            _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
            _requestAccessor = requestAccessor ?? throw new ArgumentNullException(nameof(requestAccessor));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        }

        private IUmbracoContext CreateUmbracoContext()
        {
            // TODO: It is strange having the IVariationContextAccessor initialized here and piggy backing off of IUmbracoContext.
            // There's no particular reason that IVariationContextAccessor needs to exist as part of IUmbracoContext.
            // Making this change however basically means that anywhere EnsureUmbracoContext is called, the IVariationContextAccessor
            // would most likely need to be initialized too. This can easily happen in middleware for each request, however
            // EnsureUmbracoContext is called for running on background threads too and it would be annoying to have to also ensure
            // IVariationContextAccessor. Hrm.

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
                _backofficeSecurityAccessor.BackOfficeSecurity,
                _umbracoRequestPaths,
                _hostingEnvironment,
                _variationContextAccessor,
                _uriUtility,
                _cookieManager,
                _requestAccessor);
        }

        /// <inheritdoc />
        public UmbracoContextReference EnsureUmbracoContext()
        {
            IUmbracoContext currentUmbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (currentUmbracoContext != null)
            {
                return new UmbracoContextReference(currentUmbracoContext, false, _umbracoContextAccessor);
            }

            IUmbracoContext umbracoContext = CreateUmbracoContext();
            _umbracoContextAccessor.UmbracoContext = umbracoContext;

            return new UmbracoContextReference(umbracoContext, true, _umbracoContextAccessor);
        }

    }
}
