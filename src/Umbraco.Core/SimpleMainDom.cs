using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides a simple implementation of <see cref="IMainDom" />.
/// </summary>
public class SimpleMainDom : IMainDom, IDisposable
{
    private readonly List<KeyValuePair<int, Action>> _callbacks = new();
    private readonly object _locko = new();
    private bool _disposedValue;
    private bool _isStopping;

    /// <inheritdoc />
    public bool IsMainDom { get; private set; } = true;

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // always acquire
    public bool Acquire(IApplicationShutdownRegistry hostingEnvironment) => true;

    /// <inheritdoc />
    public bool Register(Action? install, Action? release, int weight = 100)
    {
        lock (_locko)
        {
            if (_isStopping)
            {
                return false;
            }

            install?.Invoke();
            if (release != null)
            {
                _callbacks.Add(new KeyValuePair<int, Action>(weight, release));
            }

            return true;
        }
    }

    public void Stop()
    {
        lock (_locko)
        {
            if (_isStopping)
            {
                return;
            }

            if (IsMainDom == false)
            {
                return; // probably not needed
            }

            _isStopping = true;
        }

        try
        {
            foreach (Action callback in _callbacks.OrderBy(x => x.Key).Select(x => x.Value))
            {
                callback(); // no timeout on callbacks
            }
        }
        finally
        {
            // in any case...
            IsMainDom = false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Stop();
            }

            _disposedValue = true;
        }
    }
}
