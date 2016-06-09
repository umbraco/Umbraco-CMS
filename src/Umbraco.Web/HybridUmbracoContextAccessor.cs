using System;

namespace Umbraco.Web
{
    internal class HybridUmbracoContextAccessor : IUmbracoContextAccessor
    {
        private const string HttpContextItemKey = "Umbraco.Web.UmbracoContext";
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ThreadStatic]
        private static UmbracoContext _umbracoContext;

        public HybridUmbracoContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UmbracoContext UmbracoContext
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return _umbracoContext; //throw new Exception("oops:httpContext");
                return (UmbracoContext) httpContext.Items[HttpContextItemKey];
            }

            set
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) //throw new Exception("oops:httpContext");
                {
                    _umbracoContext = value;
                    return;
                }
                httpContext.Items[HttpContextItemKey] = value;
            }
        }
    }
}
