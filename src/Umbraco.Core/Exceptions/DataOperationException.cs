using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="System.Exception" />
[Serializable]
public class DataOperationException<T> : Exception
    where T : Enum
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataOperationException{T}" /> class.
    /// </summary>
    public DataOperationException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataOperationException{T}" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DataOperationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataOperationException{T}" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public DataOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataOperationException{T}" /> class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    public DataOperationException(T operation)
        : this(operation, "Data operation exception: " + operation)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataOperationException{T}" /> class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="message">The message.</param>
    public DataOperationException(T operation, string message)
        : base(message) =>
        Operation = operation;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataOperationException{T}" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    /// <exception cref="ArgumentNullException">info</exception>
    protected DataOperationException(SerializationInfo info, StreamingContext context)
        : base(info, context) =>
        Operation = (T)Enum.Parse(typeof(T), info.GetString(nameof(Operation)) ?? string.Empty);

    /// <summary>
    ///     Gets the operation.
    /// </summary>
    /// <value>
    ///     The operation.
    /// </value>
    /// <remarks>
    ///     This object should be serializable to prevent a <see cref="SerializationException" /> to be thrown.
    /// </remarks>
    public T? Operation { get; private set; }

    /// <summary>
    ///     When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with
    ///     information about the exception.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    /// <exception cref="ArgumentNullException">info</exception>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        info.AddValue(nameof(Operation), Operation is not null ? Enum.GetName(typeof(T), Operation) : string.Empty);

        base.GetObjectData(info, context);
    }
}
