using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    public class ContextUrlProviderFactory : IContextUrlProviderFactory
    {
        private readonly IWebRoutingSection _routingSettings;
        private readonly IEnumerable<IUrlProvider> _urlProviders;
        private readonly IEnumerable<IMediaUrlProvider> _mediaUrlProviders;
        private readonly IVariationContextAccessor _variationContextAccessor;
        /// <summary>
        /// Factory for Initializing a new instance of the <see cref="UrlProvider"/> class with an Umbraco context and a list of URL providers.
        /// </summary>
        /// <param name="routingSettings"></param>
        /// <param name="urlProviders"></param>
        /// <param name="mediaUrlProviders"></param>
        /// <param name="variationContextAccessor"></param>
        public ContextUrlProviderFactory(IWebRoutingSection routingSettings, IEnumerable<IUrlProvider> urlProviders, IEnumerable<IMediaUrlProvider> mediaUrlProviders, IVariationContextAccessor variationContextAccessor)
        {
            if (routingSettings == null) throw new ArgumentNullException(nameof(routingSettings));
            _routingSettings = routingSettings;
            _urlProviders = urlProviders;
            _mediaUrlProviders = mediaUrlProviders;
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProvider"/> class with an Umbraco context and a list of URL providers.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        public UrlProvider Build(UmbracoContext umbracoContext)
        {
            return new UrlProvider(umbracoContext, _routingSettings, _urlProviders, _mediaUrlProviders, _variationContextAccessor);
        }
    }
}
