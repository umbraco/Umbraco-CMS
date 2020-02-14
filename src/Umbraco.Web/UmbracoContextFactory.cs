using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    /// <summary>
    /// Creates and manages <see cref="IUmbracoContext"/> instances.
    /// </summary>
    public class UmbracoContextFactory : IUmbracoContextFactory
    {
        private static readonly NullWriter NullWriterInstance = new NullWriter();

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;

        private readonly IGlobalSettings _globalSettings;
        private readonly IUserService _userService;
        private readonly IIOHelper _ioHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPublishedUrlProvider _publishedUrlProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoContextFactory"/> class.
        /// </summary>
        public UmbracoContextFactory(IUmbracoContextAccessor umbracoContextAccessor, IPublishedSnapshotService publishedSnapshotService, IVariationContextAccessor variationContextAccessor, IDefaultCultureAccessor defaultCultureAccessor, IGlobalSettings globalSettings, IUserService userService, IIOHelper ioHelper, IHttpContextAccessor httpContextAccessor, IPublishedUrlProvider publishedUrlProvider)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _defaultCultureAccessor = defaultCultureAccessor ?? throw new ArgumentNullException(nameof(defaultCultureAccessor));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _ioHelper = ioHelper;
            _httpContextAccessor = httpContextAccessor;
            _publishedUrlProvider = publishedUrlProvider;
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


            var webSecurity = new WebSecurity(_httpContextAccessor, _userService, _globalSettings, _ioHelper);

            return new UmbracoContext(_httpContextAccessor, _publishedSnapshotService, webSecurity, _globalSettings, _variationContextAccessor, _ioHelper, _publishedUrlProvider);
        }

        /// <inheritdoc />
        public UmbracoContextReference EnsureUmbracoContext()
        {
            var currentUmbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (currentUmbracoContext != null)
                return new UmbracoContextReference(false, _umbracoContextAccessor);


            var umbracoContext = CreateUmbracoContext();
            _umbracoContextAccessor.UmbracoContext = umbracoContext;

            return new UmbracoContextReference(true, _umbracoContextAccessor);
        }

        // dummy TextWriter that does not write
        private class NullWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
