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

                if (httpContext is null)
                {
                    throw new InvalidOperationException("HttpContext is not available");
                }

                return new HttpContextWrapper(httpContext);
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
