using System;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    internal static class ContentTypeBaseServiceProviderExtensions
    {
        //TODO: Maybe this should just be on the IContentTypeBaseServiceProvider interface?
        public static IContentTypeComposition GetContentTypeOf(this IContentTypeBaseServiceProvider serviceProvider, IContentBase contentBase)
        {
            if (contentBase == null) throw new ArgumentNullException(nameof(contentBase));
            return serviceProvider.For(contentBase)?.Get(contentBase.ContentTypeId);
        }
    }
}
