using System;
using System.Web;

namespace Umbraco.Web
{
    internal class AspNetHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext
        {
            get
            {
                return HttpContext.Current;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
