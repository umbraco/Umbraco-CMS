namespace Umbraco.Cms.Core;

/// <summary>
///     Provides an equivalent to the c# lock statement, to be used in a using block.
/// </summary>
/// <remarks>Ie replace <c>lock (o) {...}</c> by <c>using (new MonitorLock(o)) { ... }</c></remarks>
public class MonitorLock : IDisposable
{
    private readonly bool _entered;
    private readonly object _locker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MonitorLock" /> class with an object to lock.
    /// </summary>
    /// <param name="locker">The object to lock.</param>
    /// <remarks>Should always be used within a using block.</remarks>
    public MonitorLock(object locker)
    {
        _locker = locker;
        _entered = false;
        Monitor.Enter(_locker, ref _entered);
    }

    void IDisposable.Dispose()
    {
        if (_entered)
        {
            Monitor.Exit(_locker);
        }
    }
}
