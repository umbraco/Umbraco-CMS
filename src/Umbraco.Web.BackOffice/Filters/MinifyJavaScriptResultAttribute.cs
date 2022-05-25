using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Web.BackOffice.ActionResults;

namespace Umbraco.Cms.Web.BackOffice.Filters;

public class MinifyJavaScriptResultAttribute : ActionFilterAttribute
{
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // logic before action goes here
        IServiceProvider serviceProvider = context.HttpContext.RequestServices;
        IHostingEnvironment? hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();
        if (!hostingEnvironment?.IsDebugMode ?? false)
        {
            IRuntimeMinifier? runtimeMinifier = serviceProvider.GetService<IRuntimeMinifier>();

            if (context.Result is JavaScriptResult jsResult)
            {
                var result = jsResult.Content;
                if (runtimeMinifier is not null)
                {
                    var minified = await runtimeMinifier.MinifyAsync(result, AssetType.Javascript);
                    jsResult.Content = minified;
                }
            }
        }

        await next(); // the actual action

        // logic after the action goes here
    }
}
