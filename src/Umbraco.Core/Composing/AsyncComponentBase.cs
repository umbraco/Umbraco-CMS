namespace Umbraco.Cms.Core.Composing;

/// <inheritdoc />
/// <remarks>
/// By default, the component will not execute if Umbraco is restarting.
/// </remarks>
public abstract class AsyncComponentBase : IAsyncComponent
{
    /// <inheritdoc />
    public async Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        if (CanExecute(isRestarting))
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        if (CanExecute(isRestarting))
        {
            await TerminateAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Determines whether the component can execute.
    /// </summary>
    /// <param name="isRestarting">If set to <c>true</c> indicates Umbraco is restarting.</param>
    /// <returns>
    ///   <c>true</c> if the component can execute; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool CanExecute(bool isRestarting)
        => isRestarting is false;

    /// <summary>
    /// Initializes the component.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token. Cancellation indicates that the start process has been aborted.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    protected abstract Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Terminates the component.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token. Cancellation indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    protected virtual Task TerminateAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
