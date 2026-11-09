using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.Api.Management.Filters;

/// <summary>
/// An action filter that delays the response until a minimum amount of time has elapsed since the action started executing.
/// </summary>
/// <remarks>
/// Giving every response the same minimum duration prevents an endpoint's timing from revealing which path it took,
/// for example whether a submitted email address belongs to a user.
/// </remarks>
internal abstract class EnsureMinimumResponseTimeFilter : IAsyncActionFilter
{
    private readonly TimeSpan _minimumResponseTime;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnsureMinimumResponseTimeFilter"/> class using the system clock.
    /// </summary>
    /// <param name="minimumResponseTime">The minimum time a response takes, measured from when the action starts executing.</param>
    protected EnsureMinimumResponseTimeFilter(TimeSpan minimumResponseTime)
        : this(minimumResponseTime, TimeProvider.System)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnsureMinimumResponseTimeFilter"/> class.
    /// </summary>
    /// <param name="minimumResponseTime">The minimum time a response takes, measured from when the action starts executing.</param>
    /// <param name="timeProvider">The time provider used to measure the elapsed time and to delay the response.</param>
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
