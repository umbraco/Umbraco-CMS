using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Lifetime
{
    public interface IUmbracoRequestLifetimeManager : IUmbracoRequestLifetime
    {
        void InitRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}
