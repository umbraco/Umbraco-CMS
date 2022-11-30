// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Lucene.Net.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An abstract provider containing the basic functionality to be able to query against Umbraco data.
/// </summary>
public abstract class UmbracoExamineIndex : LuceneIndex, IUmbracoIndex, IIndexDiagnostics
{
    private readonly UmbracoExamineIndexDiagnostics _diagnostics;
    private readonly ILogger<UmbracoExamineIndex> _logger;
    private readonly IRuntimeState _runtimeState;
    private bool _hasLoggedInitLog;

    protected UmbracoExamineIndex(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
        : base(loggerFactory, name, indexOptions)
    {
        _runtimeState = runtimeState;
        _diagnostics = new UmbracoExamineIndexDiagnostics(this, loggerFactory.CreateLogger<UmbracoExamineIndexDiagnostics>(), hostingEnvironment, indexOptions);
        _logger = loggerFactory.CreateLogger<UmbracoExamineIndex>();
    }

    public Attempt<string?> IsHealthy() => _diagnostics.IsHealthy();
    public virtual IReadOnlyDictionary<string, object?> Metadata => _diagnostics.Metadata;

    /// <summary>
    ///     When set to true Umbraco will keep the index in sync with Umbraco data automatically
    /// </summary>
    public bool EnableDefaultEventHandler { get; set; } = true;

    public bool PublishedValuesOnly { get; protected set; } = false;

    /// <summary>
    ///     override to check if we can actually initialize.
    /// </summary>
    /// <remarks>
    ///     This check is required since the base examine lib will try to rebuild on startup
    /// </remarks>
    protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs>? onComplete)
    {
        if (CanInitialize())
        {
            base.PerformDeleteFromIndex(itemIds, onComplete);
        }
    }

    protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
    {
        if (CanInitialize())
        {
            base.PerformIndexItems(values, onComplete);
        }
    }

    /// <summary>
    ///     Returns true if the Umbraco application is in a state that we can initialize the examine indexes
    /// </summary>
    /// <returns></returns>
    protected bool CanInitialize()
    {
        var canInit = _runtimeState.Level == RuntimeLevel.Run;

        if (!canInit && !_hasLoggedInitLog)
        {
            _hasLoggedInitLog = true;
            _logger.LogWarning("Runtime state is not " + RuntimeLevel.Run + ", no indexing will occur");
        }

        return canInit;
    }

    /// <summary>
    ///     This ensures that the special __Raw_ fields are indexed correctly
    /// </summary>
    /// <param name="docArgs"></param>
    protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
    {
        Document? d = docArgs.Document;

        foreach (KeyValuePair<string, IReadOnlyList<object>> f in docArgs.ValueSet.Values
                     .Where(x => x.Key.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)).ToList())
        {
            if (f.Value.Count > 0)
            {
                //remove the original value so we can store it the correct way
                d.RemoveField(f.Key);

                d.Add(new StoredField(f.Key, f.Value[0].ToString()));
            }
        }

        base.OnDocumentWriting(docArgs);
    }

    protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
    {
        base.OnTransformingIndexValues(e);

        var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value);

        //ensure special __Path field
        var path = e.ValueSet.GetValue("path");
        if (path != null)
        {
            updatedValues[UmbracoExamineFieldNames.IndexPathFieldName] = path.Yield();
        }

        //icon
        if (e.ValueSet.Values.TryGetValue("icon", out IReadOnlyList<object>? icon) &&
            e.ValueSet.Values.ContainsKey(UmbracoExamineFieldNames.IconFieldName) == false)
        {
            updatedValues[UmbracoExamineFieldNames.IconFieldName] = icon;
        }

        e.SetValues(updatedValues);
    }
}
