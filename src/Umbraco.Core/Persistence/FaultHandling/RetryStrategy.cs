﻿using System;
using Umbraco.Core.Persistence.FaultHandling.Strategies;

namespace Umbraco.Core.Persistence.FaultHandling
{
    /// <summary>
    /// Defines a callback delegate that will be invoked whenever a retry condition is encountered.
    /// </summary>
    /// <param name="retryCount">The current retry attempt count.</param>
    /// <param name="lastException">The exception which caused the retry conditions to occur.</param>
    /// <param name="delay">The delay indicating how long the current thread will be suspended for before the next iteration will be invoked.</param>
    /// <returns>Returns a callback delegate that will be invoked whenever to retry should be attempt.</returns>
    public delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);

    /// <summary>
    /// Represents a retry strategy that determines how many times should be retried and the interval between retries.
    /// </summary>
    public abstract class RetryStrategy
    {
        #region Public members
        /// <summary>
        /// The default number of retry attempts.
        /// </summary>
        public static readonly int DefaultClientRetryCount = 10;

        /// <summary>
        /// The default amount of time used when calculating a random delta in the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultClientBackoff = TimeSpan.FromSeconds(10.0);

        /// <summary>
        /// The default maximum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMaxBackoff = TimeSpan.FromSeconds(30.0);

        /// <summary>
        /// The default minimum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMinBackoff = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// The default amount of time defining an interval between retries.
        /// </summary>
        public static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// The default amount of time defining a time increment between retry attempts in the progressive delay policy.
        /// </summary>
        public static readonly TimeSpan DefaultRetryIncrement = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// The default flag indicating whether or not the very first retry attempt will be made immediately
        /// whereas the subsequent retries will remain subject to retry interval.
        /// </summary>
        public static readonly bool DefaultFirstFastRetry = true;

        #endregion

        /// <summary>
        /// Returns a default policy that does no retries, it just invokes action exactly once.
        /// </summary>
        public static readonly RetryStrategy NoRetry = new FixedInterval(0, DefaultRetryInterval);

        /// <summary>
        /// Returns a default policy that implements a fixed retry interval configured with <see cref="RetryStrategy.DefaultClientRetryCount"/> and <see cref="RetryStrategy.DefaultRetryInterval"/> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryStrategy DefaultFixed = new FixedInterval(DefaultClientRetryCount, DefaultRetryInterval);

        /// <summary>
        /// Returns a default policy that implements a progressive retry interval configured with <see cref="RetryStrategy.DefaultClientRetryCount"/>, <see cref="RetryStrategy.DefaultRetryInterval"/> and <see cref="RetryStrategy.DefaultRetryIncrement"/> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryStrategy DefaultProgressive = new Incremental(DefaultClientRetryCount, DefaultRetryInterval, DefaultRetryIncrement);

        /// <summary>
        /// Returns a default policy that implements a random exponential retry interval configured with <see cref="RetryStrategy.DefaultClientRetryCount"/>, <see cref="RetryStrategy.DefaultMinBackoff"/>, <see cref="RetryStrategy.DefaultMaxBackoff"/> and <see cref="RetryStrategy.DefaultClientBackoff"/> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryStrategy DefaultExponential = new ExponentialBackoff(DefaultClientRetryCount, DefaultMinBackoff, DefaultMaxBackoff, DefaultClientBackoff);

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryStrategy"/> class.
        /// </summary>
        /// <param name="name">The name of the retry strategy.</param>
        /// <param name="firstFastRetry">a value indicating whether or not the very first retry attempt will be made immediately
        /// whereas the subsequent retries will remain subject to retry interval.</param>
        protected RetryStrategy(string name, bool firstFastRetry)
        {
            this.Name = name;
            this.FastFirstRetry = firstFastRetry;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the very first retry attempt will be made immediately
        /// whereas the subsequent retries will remain subject to retry interval.
        /// </summary>
        public bool FastFirstRetry { get; set; }

        /// <summary>
        /// Gets the name of the retry strategy.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the corresponding ShouldRetry delegate.
        /// </summary>
        /// <returns>The ShouldRetry delegate.</returns>
        public abstract ShouldRetry GetShouldRetry();
    }
}
