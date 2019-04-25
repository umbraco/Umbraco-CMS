using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    /// <summary>
    /// Creates and manages <see cref="UmbracoContext"/> instances.
    /// </summary>
    public class UmbracoContextFactory : IUmbracoContextFactory
    {
        private static readonly NullWriter NullWriterInstance = new NullWriter();

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;

        private readonly IUmbracoSettingsSection _umbracoSettings;
        private readonly IGlobalSettings _globalSettings;
        private readonly UrlProviderCollection _urlProviders;
        private readonly MediaUrlProviderCollection _mediaUrlProviders;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoContextFactory"/> class.
        /// </summary>
        public UmbracoContextFactory(IUmbracoContextAccessor umbracoContextAccessor, IPublishedSnapshotService publishedSnapshotService, IVariationContextAccessor variationContextAccessor, IDefaultCultureAccessor defaultCultureAccessor, IUmbracoSettingsSection umbracoSettings, IGlobalSettings globalSettings, UrlProviderCollection urlProviders, MediaUrlProviderCollection mediaUrlProviders, IUserService userService)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _defaultCultureAccessor = defaultCultureAccessor ?? throw new ArgumentNullException(nameof(defaultCultureAccessor));

            _umbracoSettings = umbracoSettings ?? throw new ArgumentNullException(nameof(umbracoSettings));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _urlProviders = urlProviders ?? throw new ArgumentNullException(nameof(urlProviders));
            _mediaUrlProviders = mediaUrlProviders ?? throw new ArgumentNullException(nameof(mediaUrlProviders));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private UmbracoContext CreateUmbracoContext(HttpContextBase httpContext)
        {
            // make sure we have a variation context
            if (_variationContextAccessor.VariationContext == null)
                _variationContextAccessor.VariationContext = new VariationContext(_defaultCultureAccessor.DefaultCulture);

            var webSecurity = new WebSecurity(httpContext, _userService, _globalSettings);

            return new UmbracoContext(httpContext, _publishedSnapshotService, webSecurity, _umbracoSettings, _urlProviders, _mediaUrlProviders, _globalSettings, _variationContextAccessor);
        }

        /// <inheritdoc />
        public UmbracoContextReference EnsureUmbracoContext(HttpContextBase httpContext = null)
        {
            var currentUmbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (currentUmbracoContext != null)
                return new UmbracoContextReference(currentUmbracoContext, false, _umbracoContextAccessor);


            httpContext = httpContext ?? new HttpContextWrapper(HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("null.aspx", "", NullWriterInstance)));

            var umbracoContext = CreateUmbracoContext(httpContext);
            _umbracoContextAccessor.UmbracoContext = umbracoContext;

            return new UmbracoContextReference(umbracoContext, true, _umbracoContextAccessor);
        }

        // dummy TextWriter that does not write
        private class NullWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
