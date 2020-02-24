using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public class UmbracoBackOfficeMiddleware
    {
        private readonly RequestDelegate _next;
        public UmbracoBackOfficeMiddleware(RequestDelegate next)
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
