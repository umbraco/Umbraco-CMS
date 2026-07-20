using Examine;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

internal sealed class IndexCommitMonitor : IIndexCommitMonitor
{
    private static readonly TimeSpan _commitTimeout = TimeSpan.FromSeconds(30);

    private readonly IExamineManager _examineManager;
    private readonly ILogger<IndexCommitMonitor> _logger;

    public IndexCommitMonitor(IExamineManager examineManager, ILogger<IndexCommitMonitor> logger)
    {
        _examineManager = examineManager;
        _logger = logger;
    }

    public async Task<bool> WaitForCommitAsync(string indexAlias, CancellationToken cancellationToken)
    {
        if (_examineManager.TryGetIndex(indexAlias, out IIndex? index) is false || index is not LuceneIndex luceneIndex)
        {
            _logger.LogWarning("Could not access Lucene index for index alias: {indexAlias} - assuming successful commit by default.", indexAlias);
            return true;
        }

        if (index is IIndexStats stats && stats.GetDocumentCount() > 0)
        {
            return true;
        }

        var committed = false;
        EventHandler onCommitted = (_, _) => committed = true;

        try
        {
            luceneIndex.IndexCommitted += onCommitted;

            // Re-check after subscribing to avoid a race where the commit happened
            // between the initial check and subscribing to the event.
            if (index is IIndexStats statsAfterSubscribe && statsAfterSubscribe.GetDocumentCount() > 0)
            {
                return true;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (!committed && stopwatch.Elapsed < _commitTimeout)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            stopwatch.Stop();
            return committed;
        }
        finally
        {
            luceneIndex.IndexCommitted -= onCommitted;
        }
    }
}
