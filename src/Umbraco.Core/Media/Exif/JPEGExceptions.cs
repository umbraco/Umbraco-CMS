using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     The exception that is thrown when the format of the JPEG file could not be understood.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class NotValidJPEGFileException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidJPEGFileException" /> class.
    /// </summary>
    public NotValidJPEGFileException()
        : base("Not a valid JPEG file.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidJPEGFileException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotValidJPEGFileException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidJPEGFileException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public NotValidJPEGFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidJPEGFileException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected NotValidJPEGFileException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

/// <summary>
///     The exception that is thrown when the format of the TIFF file could not be understood.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class NotValidTIFFileException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFileException" /> class.
    /// </summary>
    public NotValidTIFFileException()
        : base("Not a valid TIFF file.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFileException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotValidTIFFileException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFileException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public NotValidTIFFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFileException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected NotValidTIFFileException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

/// <summary>
///     The exception that is thrown when the length of a section exceeds 64 kB.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class SectionExceeds64KBException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SectionExceeds64KBException" /> class.
    /// </summary>
    public SectionExceeds64KBException()
        : base("Section length exceeds 64 kB.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SectionExceeds64KBException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SectionExceeds64KBException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SectionExceeds64KBException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public SectionExceeds64KBException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SectionExceeds64KBException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected SectionExceeds64KBException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

/// <summary>
///     The exception that is thrown when the format of the TIFF header could not be understood.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
internal class NotValidTIFFHeader : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFHeader" /> class.
    /// </summary>
    public NotValidTIFFHeader()
        : base("Not a valid TIFF header.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFHeader" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotValidTIFFHeader(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFHeader" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public NotValidTIFFHeader(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidTIFFHeader" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected NotValidTIFFHeader(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
