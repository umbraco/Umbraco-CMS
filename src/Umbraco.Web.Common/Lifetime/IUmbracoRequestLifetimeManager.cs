using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Lifetime
{
    public interface IUmbracoRequestLifetimeManager
    {
        void InitRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}
