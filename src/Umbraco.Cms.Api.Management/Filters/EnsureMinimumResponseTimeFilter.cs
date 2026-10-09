using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.Api.Management.Filters;

internal abstract class EnsureMinimumResponseTimeFilter : IAsyncActionFilter
{
    private readonly TimeSpan _minimumResponseTime;
    private readonly TimeProvider _timeProvider;

    protected EnsureMinimumResponseTimeFilter(TimeSpan minimumResponseTime)
        : this(minimumResponseTime, TimeProvider.System)
    {
    }

    protected EnsureMinimumResponseTimeFilter(TimeSpan minimumResponseTime, TimeProvider timeProvider)
    {
        _minimumResponseTime = minimumResponseTime;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Ensures that the action execution takes at least a minimum amount of time by delaying the response if necessary.
    /// </summary>
    /// <param name="context">The context for the action executing.</param>
    /// <param name="next">The delegate to execute the next action filter or action.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var startTimestamp = _timeProvider.GetTimestamp();
        await next();

        TimeSpan forceWait = _minimumResponseTime - _timeProvider.GetElapsedTime(startTimestamp);

        if (forceWait > TimeSpan.Zero)
        {
            await Task.Delay(forceWait, _timeProvider).ConfigureAwait(false);
        }
    }
}
