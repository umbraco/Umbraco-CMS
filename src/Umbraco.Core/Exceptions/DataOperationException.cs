using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="System.Exception" />
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
    ///     Gets the operation.
    /// </summary>
    /// <value>
    ///     The operation.
    /// </value>
    /// <remarks>
    ///     This object should be serializable to prevent a <see cref="SerializationException" /> to be thrown.
    /// </remarks>
    public T? Operation { get; private set; }
}
