using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.Api.Management.Filters;

internal abstract class EnsureMinimumResponseTimeFilter : IAsyncActionFilter
{
    private readonly TimeSpan _minimumResponseTime;

    protected EnsureMinimumResponseTimeFilter(TimeSpan minimumResponseTime) => _minimumResponseTime = minimumResponseTime;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        await next();
        stopwatch.Stop();

        TimeSpan forceWait = _minimumResponseTime.Subtract(stopwatch.Elapsed);

        if (forceWait.Microseconds > 0)
        {
            await Task.Delay(forceWait).ConfigureAwait(false);
        }
    }
}
