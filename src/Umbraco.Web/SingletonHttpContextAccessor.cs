using System.Web;

namespace Umbraco.Web
{
    internal class SingletonHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContextBase Value
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }
    }
}