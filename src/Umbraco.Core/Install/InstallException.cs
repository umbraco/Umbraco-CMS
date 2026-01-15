using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install;

/// <summary>
///     Used for steps to be able to return a JSON structure back to the UI.
/// </summary>
/// <seealso cref="System.Exception" />
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
}
