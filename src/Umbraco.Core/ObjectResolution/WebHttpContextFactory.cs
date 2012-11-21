using System.Web;

namespace Umbraco.Core.ObjectResolution
{
    public class WebHttpContextFactory : IHttpContextFactory
    {
        public HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }
    }
}