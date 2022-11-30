using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     The exception that is thrown when a migration expression is not executed.
/// </summary>
/// <remarks>
///     Migration expressions such as Alter.Table(...).Do() must end with Do(), else they are not executed.
///     When a non-executed expression is detected, an IncompleteMigrationExpressionException is thrown.
/// </remarks>
/// <seealso cref="Exception" />
[Serializable]
public class IncompleteMigrationExpressionException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IncompleteMigrationExpressionException" /> class.
    /// </summary>
    public IncompleteMigrationExpressionException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IncompleteMigrationExpressionException" /> class with a message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public IncompleteMigrationExpressionException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IncompleteMigrationExpressionException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public IncompleteMigrationExpressionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IncompleteMigrationExpressionException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected IncompleteMigrationExpressionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
