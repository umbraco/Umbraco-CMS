namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
///     Mutual exclusion per key. A key's lock exists only while a thread holds it or waits for it.
/// </summary>
/// <remarks>
///     Not reentrant: a thread that enters a key it already holds deadlocks.
/// </remarks>
internal sealed class KeyedLock<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, Entry> _entries = new();

    /// <summary>
    ///     Gets the number of keys currently held or waited for.
    /// </summary>
    internal int Count
    {
        get
        {
            lock (_entries)
            {
                return _entries.Count;
            }
        }
    }

    /// <summary>
    ///     Blocks until the key's lock is free and takes it. Dispose the result to release it.
    /// </summary>
    public IDisposable Enter(TKey key)
    {
        Entry entry;
        lock (_entries)
        {
            if (_entries.TryGetValue(key, out Entry? existing))
            {
                entry = existing;
            }
            else
            {
                entry = new Entry();
                _entries[key] = entry;
            }

            entry.Users++;
        }

        entry.Semaphore.Wait();
        return new Releaser(this, key, entry);
    }

    private void Exit(TKey key, Entry entry)
    {
        entry.Semaphore.Release();
        lock (_entries)
        {
            if (--entry.Users == 0)
            {
                _entries.Remove(key);
                entry.Semaphore.Dispose();
            }
        }
    }

    private sealed class Entry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int Users { get; set; }
    }

    private sealed class Releaser(KeyedLock<TKey> owner, TKey key, Entry entry) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owner.Exit(key, entry);
        }
    }
}
