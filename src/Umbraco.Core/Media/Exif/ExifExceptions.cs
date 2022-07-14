using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     The exception that is thrown when the format of the JPEG/EXIF file could not be understood.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class NotValidExifFileException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidExifFileException" /> class.
    /// </summary>
    public NotValidExifFileException()
        : base("Not a valid JPEG/EXIF file.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidExifFileException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotValidExifFileException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidExifFileException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public NotValidExifFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidExifFileException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected NotValidExifFileException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
