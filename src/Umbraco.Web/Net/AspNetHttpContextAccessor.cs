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
                return new HttpContextWrapper(System.Web.HttpContext.Current);
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
