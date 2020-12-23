using System;
using Umbraco.Web;

namespace Umbraco.Core
{

    public static class UmbracoContextAccessorExtensions
    {
        public static IUmbracoContext GetRequiredUmbracoContext(this IUmbracoContextAccessor umbracoContextAccessor)
        {
            if (umbracoContextAccessor == null) throw new ArgumentNullException(nameof(umbracoContextAccessor));

            var umbracoContext = umbracoContextAccessor.UmbracoContext;

            if(umbracoContext is null) throw new InvalidOperationException("UmbracoContext is null");

            return umbracoContext;
        }
    }
}
