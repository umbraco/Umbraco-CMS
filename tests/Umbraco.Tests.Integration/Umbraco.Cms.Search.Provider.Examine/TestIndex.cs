using Examine;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine;

internal class TestIndex : LuceneIndex
{
    private int _pendingOperations;
    private volatile bool _hasUncommittedWrites;

    public TestIndex(ILoggerFactory loggerFactory, string name, IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions) : base(loggerFactory, name, indexOptions)
        => IndexCommitted += OnIndexCommitted;

    /// <summary>
    /// Gets a value indicating whether every queued index or delete operation has been processed and committed.
    /// </summary>
    /// <remarks>
    /// Examine processes writes on a background queue and commits them on a debounce timer, so searchers
    /// only reliably see a write once both have happened.
    /// </remarks>
    public bool IsSettled => Volatile.Read(ref _pendingOperations) == 0 && _hasUncommittedWrites is false;

    protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs>? onComplete)
    {
        BeginOperation();
        base.PerformIndexItems(values, TrackCompletion(onComplete));
    }

    protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs>? onComplete)
    {
        BeginOperation();
        base.PerformDeleteFromIndex(itemIds, TrackCompletion(onComplete));
    }

    private void BeginOperation()
    {
        _hasUncommittedWrites = true;
        Interlocked.Increment(ref _pendingOperations);
    }

    private Action<IndexOperationEventArgs> TrackCompletion(Action<IndexOperationEventArgs>? onComplete)
        => args =>
        {
            try
            {
                onComplete?.Invoke(args);
            }
            finally
            {
                Interlocked.Decrement(ref _pendingOperations);
            }
        };

    // A commit that lands while an operation is still in flight does not cover it; that operation schedules its own commit.
    private void OnIndexCommitted(object? sender, EventArgs e)
    {
        if (Volatile.Read(ref _pendingOperations) == 0)
        {
            _hasUncommittedWrites = false;
        }
    }
}
