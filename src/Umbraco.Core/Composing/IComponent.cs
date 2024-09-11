namespace Umbraco.Cms.Core.Composing;

/// <inheritdoc />
[Obsolete("Use IAsyncComponent instead. This interface will be removed in a future version.")]
public interface IComponent : IAsyncComponent
{
    /// <summary>
    /// Initializes the component.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Terminates the component.
    /// </summary>
    void Terminate();

    /// <inheritdoc />
    Task IAsyncComponent.InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        Initialize();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    Task IAsyncComponent.TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        Terminate();

        return Task.CompletedTask;
    }
}
