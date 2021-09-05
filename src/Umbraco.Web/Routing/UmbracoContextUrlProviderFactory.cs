using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    public class UmbracoContextUrlProviderFactory : IUmbracoContextUrlProviderFactory
    {
        private readonly UrlProviderSettings _urlProviderSettings;
        private readonly IEnumerable<IUrlProvider> _urlProviders;
        private readonly IEnumerable<IMediaUrlProvider> _mediaUrlProviders;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public UmbracoContextUrlProviderFactory(
            UrlProviderSettings urlProviderSettings,
            IEnumerable<IUrlProvider> urlProviders,
            IEnumerable<IMediaUrlProvider> mediaUrlProviders,
            IVariationContextAccessor variationContextAccessor)
        {
            _urlProviderSettings = urlProviderSettings;
            _urlProviders = urlProviders;
            _mediaUrlProviders = mediaUrlProviders;
            _variationContextAccessor = variationContextAccessor;
        }

        /// <summary>
        /// Creates an instance of UrlProvider for the given Context
        /// </summary>
        /// <param name="umbracoContext">Umbraco Context</param>
        /// <returns>Url Provider</returns>
        public UrlProvider Create(UmbracoContext umbracoContext)
        {
            return new UrlProvider(umbracoContext, _urlProviderSettings, _urlProviders, _mediaUrlProviders, _variationContextAccessor);
        }
    }
}
