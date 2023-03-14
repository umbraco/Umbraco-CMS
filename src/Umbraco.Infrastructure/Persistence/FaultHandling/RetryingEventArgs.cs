namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

/// <summary>
///     Contains information required for the <see cref="RetryPolicy.Retrying" /> event.
/// </summary>
public class RetryingEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryingEventArgs" /> class.
    /// </summary>
    /// <param name="currentRetryCount">The current retry attempt count.</param>
    /// <param name="delay">
    ///     The delay indicating how long the current thread will be suspended for before the next iteration
    ///     will be invoked.
    /// </param>
    /// <param name="lastException">The exception which caused the retry conditions to occur.</param>
    public RetryingEventArgs(int currentRetryCount, TimeSpan delay, Exception lastException)
    {
        // Guard.ArgumentNotNull(lastException, "lastException");
        CurrentRetryCount = currentRetryCount;
        Delay = delay;
        LastException = lastException;
    }

    /// <summary>
    ///     Gets the current retry count.
    /// </summary>
    public int CurrentRetryCount { get; }

    /// <summary>
    ///     Gets the delay indicating how long the current thread will be suspended for before the next iteration will be
    ///     invoked.
    /// </summary>
    public TimeSpan Delay { get; }

    /// <summary>
    ///     Gets the exception which caused the retry conditions to occur.
    /// </summary>
    public Exception LastException { get; }
}
