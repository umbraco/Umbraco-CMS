using System.Web;

namespace Umbraco.Web
{
    public interface IHttpContextAccessor
    {
        HttpContextBase HttpContext { get; set; }
    }
}
