using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Website.AspNetCore
{
    public class UmbracoWebsiteMiddleware
    {
        private readonly RequestDelegate _next;
        public UmbracoWebsiteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
