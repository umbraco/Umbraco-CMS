using System.Web;
using Umbraco.Web;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    public class NoHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContextBase HttpContext { get; set; } = null;
    }
}
