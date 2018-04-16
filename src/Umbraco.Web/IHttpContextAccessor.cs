using System.Web;

namespace Umbraco.Web
{
    public interface IHttpContextAccessor
    {
        HttpContext HttpContext { get; set; }
    }
}
