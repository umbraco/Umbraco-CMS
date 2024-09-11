namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Represents a component.
/// </summary>
/// <remarks>
/// <para>
/// Components are created by DI and therefore must have a public constructor.
/// </para>
/// <para>
/// All components are terminated in reverse order when Umbraco terminates, and disposable components are disposed.
/// </para>
/// </remarks>
public interface IAsyncComponent
{
    /// <summary>
    /// Initializes the component.
    /// </summary>
    /// <param name="isRestarting">If set to <c>true</c> indicates Umbraco is restarting.</param>
    /// <param name="cancellationToken">The cancellation token. Cancellation indicates that the start process has been aborted.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken);

    /// <summary>
    /// Terminates the component.
    /// </summary>
    /// <param name="isRestarting">If set to <c>true</c> indicates Umbraco is restarting.</param>
    /// <param name="cancellationToken">The cancellation token. Cancellation indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken);
}
