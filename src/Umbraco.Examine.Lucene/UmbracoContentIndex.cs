// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An indexer for Umbraco content and media
/// </summary>
public class UmbracoContentIndex : UmbracoContentIndexBase, IUmbracoContentIndex
{
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

        LuceneDirectoryIndexOptions namedOptions = indexOptions.Get(name);
        if (namedOptions == null)
        {
            throw new InvalidOperationException(
                $"No named {typeof(LuceneDirectoryIndexOptions)} options with name {name}");
        }

        if (namedOptions.Validator is IContentValueSetValidator contentValueSetValidator)
        {
            PublishedValuesOnly = contentValueSetValidator.PublishedValuesOnly;
        }
    }

    protected ILocalizationService? LanguageService { get; }

    /// <summary>
    ///     Explicitly override because we need to do validation differently than the underlying logic
    /// </summary>
    /// <param name="values"></param>
    void IIndex.IndexItems(IEnumerable<ValueSet> values) => PerformIndexItems(values, OnIndexOperationComplete);

    /// <summary>
    ///     Special check for invalid paths
    /// </summary>
    /// <param name="values"></param>
    /// <param name="onComplete"></param>
    protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
    {
        // We don't want to re-enumerate this list, but we need to split it into 2x enumerables: invalid and valid items.
        // The Invalid items will be deleted, these are items that have invalid paths (i.e. moved to the recycle bin, etc...)
        // Then we'll index the Value group all together.
        var invalidOrValid = values.GroupBy(v =>
        {
            if (!v.Values.TryGetValue("path", out IReadOnlyList<object>? paths) || paths.Count <= 0 || paths[0] == null)
            {
                return ValueSetValidationStatus.Failed;
            }

            ValueSetValidationResult validationResult = ValueSetValidator.Validate(v);

            return validationResult.Status;
        }).ToArray();

        var hasDeletes = false;
        var hasUpdates = false;

        // ordering by descending so that Filtered/Failed processes first
        foreach (IGrouping<ValueSetValidationStatus, ValueSet> group in invalidOrValid.OrderByDescending(x => x.Key))
        {
            switch (group.Key)
            {
                case ValueSetValidationStatus.Valid:
                    hasUpdates = true;

                    //these are the valid ones, so just index them all at once
                    base.PerformIndexItems(group.ToArray(), onComplete);
                    break;
                case ValueSetValidationStatus.Failed:
                    // don't index anything that is invalid
                    break;
                case ValueSetValidationStatus.Filtered:
                    hasDeletes = true;

                    // these are the invalid/filtered items so we'll delete them
                    // since the path is not valid we need to delete this item in
                    // case it exists in the index already and has now
                    // been moved to an invalid parent.
                    base.PerformDeleteFromIndex(group.Select(x => x.Id), null);
                    break;
            }
        }

        if ((hasDeletes && !hasUpdates) || (!hasDeletes && !hasUpdates))
        {
            //we need to manually call the completed method
            onComplete(new IndexOperationEventArgs(this, 0));
        }
    }
}
