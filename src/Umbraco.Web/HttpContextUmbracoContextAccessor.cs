using System;

namespace Umbraco.Web
{
    internal class HttpContextUmbracoContextAccessor : IUmbracoContextAccessor
    {
        private const string HttpContextItemKey = "Umbraco.Web.UmbracoContext";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextUmbracoContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IUmbracoContext UmbracoContext
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) throw new Exception("oops:httpContext");
                return (IUmbracoContext) httpContext.Items[HttpContextItemKey];
            }

            set
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) throw new Exception("oops:httpContext");
                httpContext.Items[HttpContextItemKey] = value;
            }
        }
    }
}
