using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install;

/// <summary>
///     Used for steps to be able to return a JSON structure back to the UI.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class InstallException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallException" /> class.
    /// </summary>
    public InstallException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InstallException(string message)
        : this(message, "error", null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="viewModel">The view model.</param>
    public InstallException(string message, object viewModel)
        : this(message, "error", viewModel)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="view">The view.</param>
    /// <param name="viewModel">The view model.</param>
    public InstallException(string message, string view, object? viewModel)
        : base(message)
    {
        View = view;
        ViewModel = viewModel;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public InstallException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected InstallException(SerializationInfo info, StreamingContext context)
        : base(info, context) =>
        View = info.GetString(nameof(View));

    /// <summary>
    ///     Gets the view.
    /// </summary>
    /// <value>
    ///     The view.
    /// </value>
    public string? View { get; private set; }

    /// <summary>
    ///     Gets the view model.
    /// </summary>
    /// <value>
    ///     The view model.
    /// </value>
    /// <remarks>
    ///     This object is not included when serializing.
    /// </remarks>
    public object? ViewModel { get; private set; }

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

        info.AddValue(nameof(View), View);

        base.GetObjectData(info, context);
    }
}
