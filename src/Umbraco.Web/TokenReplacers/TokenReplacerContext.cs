using System;
using System.Web;

namespace Umbraco.Web.TokenReplacers
{
    public class TokenReplacerContext
    {
        public TokenReplacerContext(HttpContextBase httpContext, UmbracoContext umbracoContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            HttpContext = httpContext;
            UmbracoContext = umbracoContext;
        }

        public HttpContextBase HttpContext { get; private set; }

        public UmbracoContext UmbracoContext { get; private set; }
    }
}