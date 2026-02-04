using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core;

/// <summary>
///     Default implementation of <see cref="IFireAndForgetRunner" /> that runs tasks on background threads.
/// </summary>
/// <remarks>
///     This implementation suppresses the execution context flow to prevent AsyncLocal values from leaking to child threads.
/// </remarks>
public class FireAndForgetRunner : IFireAndForgetRunner
{
    private readonly ILogger<FireAndForgetRunner> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FireAndForgetRunner" /> class.
    /// </summary>
    /// <param name="logger">The logger for recording exceptions from background tasks.</param>
    public FireAndForgetRunner(ILogger<FireAndForgetRunner> logger) => _logger = logger;

    /// <inheritdoc />
    public void RunFireAndForget(Func<Task> task) => ExecuteBackgroundTask(task);

    private Task ExecuteBackgroundTask(Func<Task> fn)
    {
        // it is also possible to use UnsafeQueueUserWorkItem which does not flow the execution context,
        // however that seems more difficult to use for async operations.

        // Do not flow AsyncLocal to the child thread
        using (ExecutionContext.SuppressFlow())
        {
            // NOTE: ConfigureAwait(false) is irrelevant here, it is not needed because this is not being
            // awaited. ConfigureAwait(false) is only relevant when awaiting to prevent the SynchronizationContext
            // (very different from the ExecutionContext!) from running the continuation on the calling thread.
            return Task.Run(LoggingWrapper(fn));
        }
    }

    private Func<Task> LoggingWrapper(Func<Task> fn) =>
        async () =>
        {
            try
            {
                await fn();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown in a background thread");
            }
        };
}
