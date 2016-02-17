using System.Web;

namespace Umbraco.Web
{
    /// <summary>
    /// Used to retrieve the HttpContext
    /// </summary>
    /// <remarks>
    /// NOTE: This has a singleton lifespan
    /// </remarks>
    public interface IHttpContextAccessor
    {
        HttpContextBase Value { get; }
    }
}