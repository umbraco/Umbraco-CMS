using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
///     An exception that is thrown if an unattended installation occurs.
/// </summary>
[Serializable]
public class UnattendedInstallException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UnattendedInstallException" /> class.
    /// </summary>
    public UnattendedInstallException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnattendedInstallException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UnattendedInstallException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnattendedInstallException" /> class with a specified error message
    ///     and a reference to the inner exception which is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception, or null.</param>
    public UnattendedInstallException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnattendedInstallException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected UnattendedInstallException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
