using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    // fixme
    public static class ContentTypeBaseServiceProviderExtensions
    {
        public static IContentTypeComposition GetContentTypeOf(this IContentTypeBaseServiceProvider serviceProvider, IContentBase contentBase)
            => serviceProvider.For(contentBase).Get(contentBase.ContentTypeId);
    }
}
