namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling.Strategies;

/// <summary>
///     A retry strategy with a specified number of retry attempts and an incremental time interval between retries.
/// </summary>
public class Incremental : RetryStrategy
{
    private readonly TimeSpan _increment;
    private readonly TimeSpan _initialInterval;
    private readonly int _retryCount;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Incremental" /> class.
    /// </summary>
    public Incremental()
        : this(DefaultClientRetryCount, DefaultRetryInterval, DefaultRetryIncrement)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Incremental" /> class.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts.</param>
    /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
    /// <param name="increment">
    ///     The incremental time value that will be used for calculating the progressive delay between
    ///     retries.
    /// </param>
    public Incremental(int retryCount, TimeSpan initialInterval, TimeSpan increment)
        : this(null, retryCount, initialInterval, increment)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Incremental" /> class.
    /// </summary>
    /// <param name="name">The retry strategy name.</param>
    /// <param name="retryCount">The number of retry attempts.</param>
    /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
    /// <param name="increment">
    ///     The incremental time value that will be used for calculating the progressive delay between
    ///     retries.
    /// </param>
    public Incremental(string? name, int retryCount, TimeSpan initialInterval, TimeSpan increment)
        : this(name, retryCount, initialInterval, increment, DefaultFirstFastRetry)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Incremental" /> class.
    /// </summary>
    /// <param name="name">The retry strategy name.</param>
    /// <param name="retryCount">The number of retry attempts.</param>
    /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
    /// <param name="increment">
    ///     The incremental time value that will be used for calculating the progressive delay between
    ///     retries.
    /// </param>
    /// <param name="firstFastRetry">
    ///     a value indicating whether or not the very first retry attempt will be made immediately
    ///     whereas the subsequent retries will remain subject to retry interval.
    /// </param>
    public Incremental(string? name, int retryCount, TimeSpan initialInterval, TimeSpan increment, bool firstFastRetry)
        : base(name, firstFastRetry)
    {
        // Guard.ArgumentNotNegativeValue(retryCount, "retryCount");
        // Guard.ArgumentNotNegativeValue(initialInterval.Ticks, "initialInterval");
        // Guard.ArgumentNotNegativeValue(increment.Ticks, "increment");
        this._retryCount = retryCount;
        this._initialInterval = initialInterval;
        this._increment = increment;
    }

    /// <summary>
    ///     Returns the corresponding ShouldRetry delegate.
    /// </summary>
    /// <returns>The ShouldRetry delegate.</returns>
    public override ShouldRetry GetShouldRetry() =>
        delegate(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
        {
            if (currentRetryCount < _retryCount)
            {
                retryInterval = TimeSpan.FromMilliseconds(_initialInterval.TotalMilliseconds +
                                                          (_increment.TotalMilliseconds * currentRetryCount));

                return true;
            }

            retryInterval = TimeSpan.Zero;

            return false;
        };
}
