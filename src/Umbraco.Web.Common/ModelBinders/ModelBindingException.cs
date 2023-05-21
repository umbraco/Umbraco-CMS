using System.Runtime.Serialization;

namespace Umbraco.Cms.Web.Common.ModelBinders;

/// <summary>
///     The exception that is thrown when an error occurs while binding a source to a model.
/// </summary>
/// <seealso cref="Exception" />
/// Migrated to .NET Core
[Serializable]
public class ModelBindingException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelBindingException" /> class.
    /// </summary>
    public ModelBindingException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelBindingException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ModelBindingException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelBindingException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public ModelBindingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelBindingException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected ModelBindingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
