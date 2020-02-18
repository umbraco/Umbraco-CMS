using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public class UmbracoMiddleware
    {
        private readonly RequestDelegate _next;
        public UmbracoMiddleware(RequestDelegate next)
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
