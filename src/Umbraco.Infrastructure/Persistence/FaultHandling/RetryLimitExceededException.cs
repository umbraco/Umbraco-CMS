namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

/// <summary>
///     The special type of exception that provides managed exit from a retry loop. The user code can use this exception to
///     notify the retry policy that no further retry attempts are required.
/// </summary>
/// <seealso cref="System.Exception" />
public sealed class RetryLimitExceededException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class with a default error message.
    /// </summary>
    public RetryLimitExceededException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RetryLimitExceededException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class with a reference to the inner
    ///     exception that is the cause of this exception.
    /// </summary>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RetryLimitExceededException(Exception innerException)
        : base(null, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RetryLimitExceededException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
