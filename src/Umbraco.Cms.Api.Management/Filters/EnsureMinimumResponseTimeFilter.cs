using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.Api.Management.Filters;

internal abstract class EnsureMinimumResponseTimeFilter : IAsyncActionFilter
{
    private readonly TimeSpan _minimumResponseTime;

    protected EnsureMinimumResponseTimeFilter(TimeSpan minimumResponseTime) => _minimumResponseTime = minimumResponseTime;

    /// <summary>
    /// Ensures that the action execution takes at least a minimum amount of time by delaying the response if necessary.
    /// </summary>
    /// <param name="context">The context for the action executing.</param>
    /// <param name="next">The delegate to execute the next action filter or action.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
