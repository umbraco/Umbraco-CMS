using System.Web;
using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers
{
    public class NoHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = null;
    }
}
