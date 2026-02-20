namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Implements <see cref="IProfiler"/> as a no-operation profiler that performs no actual profiling.
/// </summary>
/// <remarks>
///     This implementation is useful when profiling is disabled or not required, providing a
///     lightweight placeholder that satisfies the <see cref="IProfiler"/> contract without overhead.
/// </remarks>
public class NoopProfiler : IProfiler
{
    private readonly VoidDisposable _disposable = new();

    /// <inheritdoc/>
    public IDisposable Step(string name) => _disposable;

    /// <inheritdoc/>
    public void Start()
    {
    }

    /// <inheritdoc/>
    public void Stop(bool discardResults = false)
    {
    }

    /// <inheritdoc/>
    public bool IsEnabled => false;

    /// <summary>
    ///     A disposable that does nothing when disposed.
    /// </summary>
    private sealed class VoidDisposable : DisposableObjectSlim
    {
        /// <inheritdoc/>
        protected override void DisposeResources()
        {
        }
    }
}
