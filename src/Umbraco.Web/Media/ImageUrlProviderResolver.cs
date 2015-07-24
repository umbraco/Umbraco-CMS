using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.Media.ImageUrlProviders;

namespace Umbraco.Web.Media
{
    internal sealed class ImageUrlProviderResolver : ManyObjectsResolverBase<ImageUrlProviderResolver, IImageUrlProvider>
    {
        internal ImageUrlProviderResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> value)
            : base(serviceProvider, logger, value)
        {
        }

        public IImageUrlProvider GetProvider(string provider)
        {
            return string.IsNullOrEmpty(provider) ?
                Values.First(v => v.Name.Equals(ImageUrlProvider.DefaultName, StringComparison.InvariantCultureIgnoreCase)) :
                Values.First(v => v.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}