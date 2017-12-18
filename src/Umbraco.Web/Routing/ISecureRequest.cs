using System.Web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Service to encapsulate if a request is executing under HTTPS
    /// </summary>
    public interface ISecureRequest
    {
        bool IsSecure(HttpRequestBase httpRequest);
        string GetScheme(HttpRequestBase request);
    }
}