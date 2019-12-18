using System.Web;
using Umbraco.Web;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    public class NoHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = null;
    }
}
