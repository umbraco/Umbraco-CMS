// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Examine.Search;
using Lucene.Net.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine.Configuration;
using Umbraco.Search.Indexing.Notifications;
using Umbraco.Search.Models;

namespace Umbraco.Search.Examine.Lucene;

/// <summary>
///     An abstract provider containing the basic functionality to be able to query against Umbraco data.
/// </summary>
public class UmbracoExamineLuceneIndex : LuceneIndex, IUmbracoExamineIndex, IIndexDiagnostics
{
    private readonly ISet<string> _idOnlyFieldSet = new HashSet<string> { "id" };
    private readonly UmbracoExamineIndexDiagnostics _diagnostics;
    private readonly ILogger<UmbracoExamineLuceneIndex> _logger;
    private readonly IRuntimeState _runtimeState;
    private bool _hasLoggedInitLog;

    public UmbracoExamineLuceneIndex(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IUmbracoIndexesConfiguration configuration,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
        : base(loggerFactory, name, indexOptions)
    {
        _runtimeState = runtimeState;
        _diagnostics = new UmbracoExamineIndexDiagnostics(this,configuration,  loggerFactory.CreateLogger<UmbracoExamineIndexDiagnostics>(), hostingEnvironment, indexOptions);
        _logger = loggerFactory.CreateLogger<UmbracoExamineLuceneIndex>();
    }

    public ISearchEngine? SearchEngine { get; } = new ExamineLuceneSearchEngine();
    public Attempt<HealthStatus?> IsHealthy() => _diagnostics.IsHealthy();
    public virtual IReadOnlyDictionary<string, object?> Metadata => _diagnostics.Metadata;

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
            var idsAsList = itemIds.ToList();

            for (var i = 0; i < idsAsList.Count; i++)
            {
                var nodeId = idsAsList[i];

                //find all descendants based on path
                var descendantPath = $@"\-1\,*{nodeId}\,*";
                var rawQuery = $"{UmbracoSearchFieldNames.IndexPathFieldName}:{descendantPath}";
                IQuery? c = Searcher.CreateQuery();
                IBooleanOperation? filtered = c.NativeQuery(rawQuery);
                IOrdering? selectedFields = filtered.SelectFields(_idOnlyFieldSet);
                ISearchResults? results = selectedFields.Execute();

                _logger.LogDebug("DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery, results.TotalItemCount);

                var toRemove = results.Select(x => x.Id).ToList();
                // delete those descendants (ensure base. is used here so we aren't calling ourselves!)
                base.PerformDeleteFromIndex(toRemove, null);

                // remove any ids from our list that were part of the descendants
                idsAsList.RemoveAll(x => toRemove.Contains(x));
            }

            base.PerformDeleteFromIndex(idsAsList, onComplete);
        }
    }

    protected override void PerformIndexItems(IEnumerable<global::Examine.ValueSet> values, Action<IndexOperationEventArgs> onComplete)
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
                     .Where(x => x.Key.StartsWith(UmbracoSearchFieldNames.RawFieldPrefix)).ToArray())
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
            updatedValues[UmbracoSearchFieldNames.IndexPathFieldName] = path.Yield();
        }

        //icon
        if (e.ValueSet.Values.TryGetValue("icon", out IReadOnlyList<object>? icon) &&
            e.ValueSet.Values.ContainsKey(UmbracoSearchFieldNames.IconFieldName) == false)
        {
            updatedValues[UmbracoSearchFieldNames.IconFieldName] = icon;
        }

        e.SetValues(updatedValues);
    }
}
