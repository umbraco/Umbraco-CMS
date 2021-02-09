using System;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core
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
