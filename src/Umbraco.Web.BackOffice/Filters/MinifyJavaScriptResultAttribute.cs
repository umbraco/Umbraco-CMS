using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.WebAssets;
using Umbraco.Web.BackOffice.ActionResults;

namespace Umbraco.Web.BackOffice.Filters
{
    public class MinifyJavaScriptResultAttribute : ActionFilterAttribute
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // logic before action goes here
            var serviceProvider = context.HttpContext.RequestServices;
            var hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();
            if (!hostingEnvironment.IsDebugMode)
            {
                var runtimeMinifier = serviceProvider.GetService<IRuntimeMinifier>();

                if (context.Result is JavaScriptResult jsResult)
                {
                    var result = jsResult.Content;
                    var minified = await runtimeMinifier.MinifyAsync(result, AssetType.Javascript);
                    jsResult.Content = minified;
                }
            }

            await next(); // the actual action

            // logic after the action goes here
        }
    }
}
