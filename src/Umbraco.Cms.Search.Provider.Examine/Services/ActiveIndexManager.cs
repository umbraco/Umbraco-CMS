using System.Collections.Concurrent;
using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Models.Configuration;
using IndexOptions = Umbraco.Cms.Search.Core.Configuration.IndexOptions;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

internal sealed class ActiveIndexManager : IActiveIndexManager
{
    private readonly ILogger<ActiveIndexManager> _logger;
    private readonly ConcurrentDictionary<string, Index> _indexes = new();

    internal const string SuffixA = "_a";
    internal const string SuffixB = "_b";

    public ActiveIndexManager(IExamineManager examineManager, ILogger<ActiveIndexManager> logger, IOptions<IndexOptions> indexOptions)
    {
        _logger = logger;

        foreach (ContentIndexRegistration registration in indexOptions.Value.GetContentIndexRegistrations())
        {
            _indexes[registration.IndexAlias] = DetermineInitialSlot(registration.IndexAlias, examineManager);
        }
    }

    public string ResolveActiveIndexName(string indexAlias)
        => indexAlias + (_indexes.TryGetValue(indexAlias, out Index? index) ? index.ActiveSuffix : SuffixA);

    public string ResolveShadowIndexName(string indexAlias)
        => indexAlias + (_indexes.TryGetValue(indexAlias, out Index? index) ? index.ShadowSuffix : SuffixB);

    public bool IsRebuilding(string indexAlias) => _indexes.TryGetValue(indexAlias, out Index? index) && index.IsRebuilding;

    public void StartRebuilding(string indexAlias)
    {
        if (_indexes.TryGetValue(indexAlias, out Index? current) is false)
        {
            _logger.LogWarning("No registered index found for {IndexAlias}, ignoring start rebuild request.", indexAlias);
            return;
        }

        if (current.IsRebuilding)
        {
            _logger.LogWarning("Rebuild already in progress for {IndexAlias}, ignoring start request.", indexAlias);
            return;
        }

        _logger.LogInformation(
            "Starting rebuild for {IndexAlias}. Active: {Active}, Shadow: {Shadow}",
            indexAlias,
            indexAlias + current.ActiveSuffix,
            indexAlias + current.ShadowSuffix);

        _indexes[indexAlias] = current with { IsRebuilding = true };
    }

    public void CompleteRebuilding(string indexAlias)
    {
        if (_indexes.TryGetValue(indexAlias, out Index? current) is false)
        {
            _logger.LogWarning("No registered index found for {IndexAlias}, ignoring complete rebuild request.", indexAlias);
            return;
        }

        if (current.IsRebuilding is false)
        {
            _logger.LogWarning("No rebuild in progress for {IndexAlias}, ignoring complete request.", indexAlias);
            return;
        }

        _logger.LogInformation(
            "Completing rebuild for {IndexAlias}. Swapping active from {OldActive} to {NewActive}.",
            indexAlias,
            indexAlias + current.ActiveSuffix,
            indexAlias + current.ShadowSuffix);

        _indexes[indexAlias] = new Index(current.ShadowSuffix, false);
    }

    public void CancelRebuilding(string indexAlias)
    {
        if (_indexes.TryGetValue(indexAlias, out Index? current) is false)
        {
            _logger.LogWarning("No registered index found for {IndexAlias}, ignoring cancel rebuild request.", indexAlias);
            return;
        }

        if (current.IsRebuilding is false)
        {
            return;
        }

        _logger.LogWarning("Cancelling rebuild for {IndexAlias}. Active index remains {Active}.", indexAlias, indexAlias + current.ActiveSuffix);
        _indexes[indexAlias] = current with { IsRebuilding = false };
    }

    private Index DetermineInitialSlot(string indexAlias, IExamineManager examineManager)
    {
        var aCount = GetDocumentCount(indexAlias + SuffixA, examineManager);
        var bCount = GetDocumentCount(indexAlias + SuffixB, examineManager);

        var activeSuffix = bCount > aCount ? SuffixB : SuffixA;

        if (aCount > 0 || bCount > 0)
        {
            _logger.LogInformation(
                "Selecting {Active} as active for {IndexAlias} (A: {ACount} docs, B: {BCount} docs).",
                indexAlias + activeSuffix, indexAlias, aCount, bCount);
        }

        return new Index(activeSuffix, false);
    }

    private static long GetDocumentCount(string physicalIndexName, IExamineManager examineManager)
        => examineManager.TryGetIndex(physicalIndexName, out IIndex? index) && index is IIndexStats stats
            ? stats.GetDocumentCount()
            : 0;

    private sealed record Index(string ActiveSuffix, bool IsRebuilding)
    {
        public string ShadowSuffix => ActiveSuffix == SuffixA ? SuffixB : SuffixA;
    }
}
