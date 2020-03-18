using System;
using System.Web;

namespace Umbraco.Web
{
    internal class AspNetHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContextBase HttpContext
        {
            get
            {
                var httpContext = System.Web.HttpContext.Current;

                return httpContext is null ? null : new HttpContextWrapper(httpContext);
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
