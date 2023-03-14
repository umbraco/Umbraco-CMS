using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
///     The exception that is thrown when authorization failed.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class AuthorizationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationException" /> class.
    /// </summary>
    public AuthorizationException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AuthorizationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public AuthorizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected AuthorizationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
