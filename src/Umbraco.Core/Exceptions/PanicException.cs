namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
///     Represents an internal exception that in theory should never been thrown, it is only thrown in circumstances that
///     should never happen.
/// </summary>
/// <seealso cref="System.Exception" />
public class PanicException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PanicException" /> class.
    /// </summary>
    public PanicException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PanicException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PanicException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PanicException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public PanicException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
