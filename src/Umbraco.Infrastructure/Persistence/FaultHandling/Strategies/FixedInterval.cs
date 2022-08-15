namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling.Strategies;

/// <summary>
///     A retry strategy with a specified number of retry attempts and a default fixed time interval between retries.
/// </summary>
public class FixedInterval : RetryStrategy
{
    private readonly int _retryCount;
    private readonly TimeSpan _retryInterval;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FixedInterval" /> class.
    /// </summary>
    public FixedInterval()
        : this(DefaultClientRetryCount)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FixedInterval" /> class.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts.</param>
    public FixedInterval(int retryCount)
        : this(retryCount, DefaultRetryInterval)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FixedInterval" /> class.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts.</param>
    /// <param name="retryInterval">The time interval between retries.</param>
    public FixedInterval(int retryCount, TimeSpan retryInterval)
        : this(null, retryCount, retryInterval, DefaultFirstFastRetry)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FixedInterval" /> class.
    /// </summary>
    /// <param name="name">The retry strategy name.</param>
    /// <param name="retryCount">The number of retry attempts.</param>
    /// <param name="retryInterval">The time interval between retries.</param>
    public FixedInterval(string name, int retryCount, TimeSpan retryInterval)
        : this(name, retryCount, retryInterval, DefaultFirstFastRetry)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FixedInterval" /> class.
    /// </summary>
    /// <param name="name">The retry strategy name.</param>
    /// <param name="retryCount">The number of retry attempts.</param>
    /// <param name="retryInterval">The time interval between retries.</param>
    /// <param name="firstFastRetry">
    ///     a value indicating whether or not the very first retry attempt will be made immediately
    ///     whereas the subsequent retries will remain subject to retry interval.
    /// </param>
    public FixedInterval(string? name, int retryCount, TimeSpan retryInterval, bool firstFastRetry)
        : base(name, firstFastRetry)
    {
        // Guard.ArgumentNotNegativeValue(retryCount, "retryCount");
        // Guard.ArgumentNotNegativeValue(retryInterval.Ticks, "retryInterval");
        this._retryCount = retryCount;
        this._retryInterval = retryInterval;
    }

    /// <summary>
    ///     Returns the corresponding ShouldRetry delegate.
    /// </summary>
    /// <returns>The ShouldRetry delegate.</returns>
    public override ShouldRetry GetShouldRetry()
    {
        if (_retryCount == 0)
        {
            return delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
            {
                interval = TimeSpan.Zero;
                return false;
            };
        }

        return delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
        {
            if (currentRetryCount < _retryCount)
            {
                interval = _retryInterval;
                return true;
            }

            interval = TimeSpan.Zero;
            return false;
        };
    }
}
