namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling.Strategies;

/// <summary>
///     A retry strategy with back-off parameters for calculating the exponential delay between retries.
/// </summary>
public class ExponentialBackoff : RetryStrategy
{
    private readonly TimeSpan _deltaBackoff;
    private readonly TimeSpan _maxBackoff;
    private readonly TimeSpan _minBackoff;
    private readonly int _retryCount;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExponentialBackoff" /> class.
    /// </summary>
    public ExponentialBackoff()
        : this(DefaultClientRetryCount, DefaultMinBackoff, DefaultMaxBackoff, DefaultClientBackoff)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExponentialBackoff" /> class.
    /// </summary>
    /// <param name="retryCount">The maximum number of retry attempts.</param>
    /// <param name="minBackoff">The minimum back-off time</param>
    /// <param name="maxBackoff">The maximum back-off time.</param>
    /// <param name="deltaBackoff">
    ///     The value that will be used for calculating a random delta in the exponential delay between
    ///     retries.
    /// </param>
    public ExponentialBackoff(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        : this(null, retryCount, minBackoff, maxBackoff, deltaBackoff, DefaultFirstFastRetry)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExponentialBackoff" /> class.
    /// </summary>
    /// <param name="name">The name of the retry strategy.</param>
    /// <param name="retryCount">The maximum number of retry attempts.</param>
    /// <param name="minBackoff">The minimum back-off time</param>
    /// <param name="maxBackoff">The maximum back-off time.</param>
    /// <param name="deltaBackoff">
    ///     The value that will be used for calculating a random delta in the exponential delay between
    ///     retries.
    /// </param>
    public ExponentialBackoff(string name, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        : this(name, retryCount, minBackoff, maxBackoff, deltaBackoff, DefaultFirstFastRetry)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExponentialBackoff" /> class.
    /// </summary>
    /// <param name="name">The name of the retry strategy.</param>
    /// <param name="retryCount">The maximum number of retry attempts.</param>
    /// <param name="minBackoff">The minimum back-off time</param>
    /// <param name="maxBackoff">The maximum back-off time.</param>
    /// <param name="deltaBackoff">
    ///     The value that will be used for calculating a random delta in the exponential delay between
    ///     retries.
    /// </param>
    /// <param name="firstFastRetry">
    ///     Indicates whether or not the very first retry attempt will be made immediately
    ///     whereas the subsequent retries will remain subject to retry interval.
    /// </param>
    public ExponentialBackoff(string? name, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, bool firstFastRetry)
        : base(name, firstFastRetry)
    {
        // Guard.ArgumentNotNegativeValue(retryCount, "retryCount");
        // Guard.ArgumentNotNegativeValue(minBackoff.Ticks, "minBackoff");
        // Guard.ArgumentNotNegativeValue(maxBackoff.Ticks, "maxBackoff");
        // Guard.ArgumentNotNegativeValue(deltaBackoff.Ticks, "deltaBackoff");
        // Guard.ArgumentNotGreaterThan(minBackoff.TotalMilliseconds, maxBackoff.TotalMilliseconds, "minBackoff");
        this._retryCount = retryCount;
        this._minBackoff = minBackoff;
        this._maxBackoff = maxBackoff;
        this._deltaBackoff = deltaBackoff;
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
                var random = new Random();

                var delta = (int)((Math.Pow(2.0, currentRetryCount) - 1.0) * random.Next(
                    (int)(_deltaBackoff.TotalMilliseconds * 0.8), (int)(_deltaBackoff.TotalMilliseconds * 1.2)));
                var interval = (int)Math.Min(_minBackoff.TotalMilliseconds + delta, _maxBackoff.TotalMilliseconds);

                retryInterval = TimeSpan.FromMilliseconds(interval);

                return true;
            }

            retryInterval = TimeSpan.Zero;
            return false;
        };
}
