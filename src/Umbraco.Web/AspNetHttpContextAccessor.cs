using System;
using System.Web;

namespace Umbraco.Web
{
    internal class AspNetHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext
        {
            get => HttpContext.Current;
            set => throw new NotSupportedException();
        }
    }
}
