using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Media;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.Media.ImageUrlProviders;

namespace Umbraco.Web.Media
{
    internal sealed class ImageUrlProviderResolver : ManyObjectsResolverBase<ImageUrlProviderResolver, IImageUrlProvider>
    {
        internal ImageUrlProviderResolver(IEnumerable<Type> value) : base(value) { }

        public IImageUrlProvider Provider(string provider)
        {
            return string.IsNullOrEmpty(provider) ? Values.First(v => v.Name == ImageUrlProvider.DefaultName) : Values.First(v => v.Name == provider);
        }
    }
}