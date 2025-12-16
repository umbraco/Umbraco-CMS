// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An indexer for Umbraco content and media.
/// </summary>
public class UmbracoContentIndex : UmbracoExamineIndex, IUmbracoContentIndex
{
    private readonly ISet<string> _idOnlyFieldSet = new HashSet<string> { "id" };
    private readonly ILogger<UmbracoContentIndex> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoContentIndex"/> class.
    /// </summary>
    public UmbracoContentIndex(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState,
        ILocalizationService? languageService = null)
        : base(loggerFactory, name, indexOptions, hostingEnvironment, runtimeState)
    {
        LanguageService = languageService;
        _logger = loggerFactory.CreateLogger<UmbracoContentIndex>();

        LuceneDirectoryIndexOptions namedOptions = indexOptions.Get(name)
            ?? throw new InvalidOperationException($"No named {typeof(LuceneDirectoryIndexOptions)} options with name {name}");

        if (namedOptions.Validator is IContentValueSetValidator contentValueSetValidator)
        {
            PublishedValuesOnly = contentValueSetValidator.PublishedValuesOnly;
            SupportProtectedContent = contentValueSetValidator.SupportProtectedContent;
        }
    }

    /// <summary>
    /// Gets the <see cref="ILocalizationService"/>.
    /// </summary>
    protected ILocalizationService? LanguageService { get; }

    /// <summary>
    ///     Explicitly override because we need to do validation differently than the underlying logic
    /// </summary>
    void IIndex.IndexItems(IEnumerable<ValueSet> values) => PerformIndexItems(values, OnIndexOperationComplete);

    /// <summary>
    ///     Special check for invalid paths.
    /// </summary>
    protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs>? onComplete)
    {
        // We don't want to re-enumerate this list, but we need to split it into 2x enumerables: invalid and valid items.
        // The Invalid items will be deleted, these are items that have invalid paths (i.e. moved to the recycle bin, etc...)
        // Then we'll index the Value group all together.
        IGrouping<ValueSetValidationStatus, ValueSet>[] invalidOrValid = values.GroupBy(v =>
        {
            if (!v.Values.TryGetValue("path", out IReadOnlyList<object>? paths) || paths.Count <= 0 || paths[0] == null)
            {
                return ValueSetValidationStatus.Failed;
            }

            if (ValueSetValidator is not null)
            {
                ValueSetValidationResult validationResult = ValueSetValidator.Validate(v);
                return validationResult.Status;
            }

            return ValueSetValidationStatus.Valid;
        }).ToArray();

        var hasDeletes = false;
        var hasUpdates = false;

        // Ordering by descending so that Filtered/Failed processes first.
        foreach (IGrouping<ValueSetValidationStatus, ValueSet> group in invalidOrValid.OrderByDescending(x => x.Key))
        {
            switch (group.Key)
            {
                case ValueSetValidationStatus.Valid:
                    hasUpdates = true;

                    // These are the valid ones, so just index them all at once.
                    base.PerformIndexItems(group.ToArray(), onComplete);
                    break;
                case ValueSetValidationStatus.Failed:
                    // Don't index anything that is invalid.
                    break;
                case ValueSetValidationStatus.Filtered:
                    hasDeletes = true;

                    // These are the invalid/filtered items so we'll delete them
                    // since the path is not valid we need to delete this item in
                    // case it exists in the index already and has now
                    // been moved to an invalid parent.
                    base.PerformDeleteFromIndex(group.Select(x => x.Id), null);
                    break;
            }
        }

        if ((hasDeletes && !hasUpdates) || (!hasDeletes && !hasUpdates))
        {
            // We need to manually call the completed method.
            onComplete?.Invoke(new IndexOperationEventArgs(this, 0));
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Deletes a node from the index.
    /// </summary>
    /// <remarks>
    ///     When a content node is deleted, we also need to delete it's children from the index so we need to perform a
    ///     custom Lucene search to find all decendents and create Delete item queues for them too.
    /// </remarks>
    /// <param name="itemIds">ID of the node to delete</param>
    /// <param name="onComplete"></param>
    protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs>? onComplete)
    {
        var idsAsList = itemIds.ToList();

        for (var i = 0; i < idsAsList.Count; i++)
        {
            var nodeId = idsAsList[i];

            // Find all descendants based on path.
            var descendantPath = $@"\-1\,*{nodeId}\,*";
            var rawQuery = $"{UmbracoExamineFieldNames.IndexPathFieldName}:{descendantPath}";
            IQuery? c = Searcher.CreateQuery();
            IBooleanOperation? filtered = c.NativeQuery(rawQuery);
            IOrdering? selectedFields = filtered.SelectFields(_idOnlyFieldSet);
            ISearchResults? results = selectedFields.Execute();
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery, results.TotalItemCount);
            }

            // Avoid unnecessary operations when we have no items to handle. This is necessary for ExamineX's Elastic implementation
            // which doesn't support making requests with an empty collection.
            if (results.TotalItemCount == 0)
            {
                continue;
            }

            var toRemove = results.Select(x => x.Id).ToList();

            // Delete those descendants (ensure base. is used here so we aren't calling ourselves!).
            base.PerformDeleteFromIndex(toRemove, null);

            // Remove any ids from our list that were part of the descendants.
            idsAsList.RemoveAll(x => toRemove.Contains(x));
        }

        // Avoid unnecessary operations when we have no items to handle. This is necessary for ExamineX's Elastic implementation
        // which doesn't support making requests with an empty collection.
        if (idsAsList.Count > 0)
        {
            base.PerformDeleteFromIndex(idsAsList, onComplete);
        }
        else
        {
            // Manually invoke the complete callback if provided, so we maintain consistency when there is or isn't anything to delete.
            onComplete?.Invoke(new IndexOperationEventArgs(this, 0));
        }
    }
}
