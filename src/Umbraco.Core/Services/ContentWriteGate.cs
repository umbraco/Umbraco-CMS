namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Admits asynchronous callers into the synchronous content write path one at a time per process.
/// </summary>
/// <remarks>
/// <para>
/// A request waiting for the content tree write lock blocks a thread for the whole wait. Under a burst of saves those
/// waiters exhaust the thread pool, which slows every await the lock holder makes and so stretches the time the lock
/// is held. Admitting one request per process into the write path, and letting the others wait asynchronously, keeps
/// the pool free. The lock itself still orders the writes across processes, so no parallelism is lost.
/// </para>
/// <para>
/// Admission is re-entrant: work started inside an admitted operation, such as a notification handler that saves
/// related content, runs without waiting again.
/// </para>
/// </remarks>
// TODO (V20): Remove once the content tree lock can be awaited, so that waiting for it no longer holds a thread.
internal sealed class ContentWriteGate
{
    private readonly SemaphoreSlim _admission = new(1, 1);
    private readonly AsyncLocal<bool> _admitted = new();

    /// <summary>
    /// Gets the gate shared by the content editing and publishing services of this process.
    /// </summary>
    public static ContentWriteGate Shared { get; } = new();

    /// <summary>
    /// Runs a synchronous operation once the caller is admitted.
    /// </summary>
    /// <typeparam name="TResult">The type of the operation's result.</typeparam>
    /// <param name="operation">The operation to run while admitted.</param>
    /// <returns>A task that completes with the operation's result once it has run.</returns>
    public async Task<TResult> RunAsync<TResult>(Func<TResult> operation)
    {
        if (_admitted.Value)
        {
            return operation();
        }

        await _admission.WaitAsync();
        try
        {
            _admitted.Value = true;
            return operation();
        }
        finally
        {
            _admitted.Value = false;
            _admission.Release();
        }
    }

    /// <summary>
    /// Runs an asynchronous operation once the caller is admitted.
    /// </summary>
    /// <typeparam name="TResult">The type of the operation's result.</typeparam>
    /// <param name="operation">The operation to run while admitted.</param>
    /// <returns>A task that completes with the operation's result once it has run.</returns>
    public async Task<TResult> RunAsync<TResult>(Func<Task<TResult>> operation)
    {
        if (_admitted.Value)
        {
            return await operation();
        }

        await _admission.WaitAsync();
        try
        {
            _admitted.Value = true;
            return await operation();
        }
        finally
        {
            _admitted.Value = false;
            _admission.Release();
        }
    }
}
