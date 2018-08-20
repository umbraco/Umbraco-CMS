using System.Web;

namespace Umbraco.Web
{
    internal class SingletonHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContextBase Value
        {
            get
            {
                var httpContext = HttpContext.Current;
                return httpContext == null ? null : new HttpContextWrapper(httpContext);
            }
        }
    }
}