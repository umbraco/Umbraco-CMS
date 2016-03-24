using System;
using System.Web;

namespace Umbraco.Web
{
    internal class DefaultHttpContextAccessor : IHttpContextAccessor
    {
        private readonly Func<HttpContextBase> _httpContext;

        public DefaultHttpContextAccessor(Func<HttpContextBase> httpContext)
        {
            _httpContext = httpContext;
        }

        public HttpContextBase Value
        {
            get { return _httpContext(); }
        }
    }
}