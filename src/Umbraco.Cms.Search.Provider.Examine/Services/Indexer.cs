using Examine;
using Examine.Search;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Provider.Examine.Configuration;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using CoreConstants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

public class Indexer : IExamineIndexer
{
    private readonly IExamineManager _examineManager;
    private readonly IActiveIndexManager _activeIndexManager;

    public Indexer(IExamineManager examineManager, IOptions<FieldOptions> fieldOptions, IActiveIndexManager activeIndexManager)
    {
        _examineManager = examineManager;
        _activeIndexManager = activeIndexManager;
        FieldOptions = fieldOptions.Value;
    }

    /// <summary>
    /// Gets the field options configuration for use in derived classes.
    /// </summary>
    protected FieldOptions FieldOptions { get; }

    /// <summary>
    /// Override this method to append custom <see cref="IndexValue"/> properties in derived classes.
    /// This method is called for each <see cref="IndexField"/> during indexing, allowing you to
    /// add custom values to the index dictionary.
    /// </summary>
    /// <param name="field">The index field being processed.</param>
    /// <param name="result">The dictionary to add custom index values to. Keys are field names, values are the data to index.</param>
    protected virtual void AppendCustomIndexValues(IndexField field, Dictionary<string, IEnumerable<object>> result)
    {
        // No-op by default. Override in derived classes to handle custom IndexValue types.
    }

    public Task AddOrUpdateAsync(
        string indexAlias,
        Guid key,
        UmbracoObjectTypes objectType,
        IEnumerable<Variation> variations,
        IEnumerable<IndexField> fields,
        ContentProtection? protection)
    {
        IIndex index = GetWriteTargetIndex(indexAlias);

        DeleteSingleDoc(index, key);

        var valuesToIndex = new List<ValueSet>();

        IEnumerable<IGrouping<string?, Variation>> variationGroups = variations.GroupBy(x => x.Culture);

        IndexField[] fieldsAsArray = fields as IndexField[] ?? fields.ToArray();

        foreach (IGrouping<string?, Variation> variationGroup in variationGroups)
        {
            var indexKey = DocumentIdHelper.CalculateDocumentId(key, variationGroup.Key);
            IEnumerable<IndexField> fieldsToMap = MapFields(fieldsAsArray.Where(x => x.Culture is null || x.Culture == variationGroup.Key), variationGroup.Key);

            valuesToIndex.Add(new ValueSet(
                indexKey,
                objectType.ToString(),
                MapToDictionary(fieldsToMap, variationGroup.Key, variationGroup.Select(x => x.Segment).Distinct(), protection)));
        }

        index.IndexItems(valuesToIndex);

        return Task.CompletedTask;
    }

    private IEnumerable<IndexField> MapFields(IEnumerable<IndexField> fields, string? culture)
    {
        var results = new Dictionary<(string FieldName, string? Segment), IndexField>();
        foreach (IndexField field in fields)
        {
            (string FieldName, string? Segment) key = (field.FieldName, field.Segment);

            if (field.Culture is null)
            {
                if (results.TryGetValue(key, out IndexField? indexField))
                {
                    results[key] = field with { Value = MergeIndexValue(indexField.Value, field.Value), Culture = culture };
                    continue;
                }

                results.Add(key, field);
            }

            if (field.Culture == culture)
            {
                if (results.TryGetValue(key, out IndexField? indexField))
                {
                    results[key] = field with { Value = MergeIndexValue(indexField.Value, field.Value) };
                    continue;
                }

                results[key] = field;
            }
        }

        return results.Select(x => x.Value);
    }

    /// <summary>
    /// Merges two <see cref="IndexValue"/> instances when the same field appears multiple times.
    /// Override this method in derived classes to handle custom <see cref="IndexValue"/> properties.
    /// </summary>
    /// <param name="original">The original IndexValue.</param>
    /// <param name="toMerge">The IndexValue to merge into the original.</param>
    /// <returns>A new IndexValue containing the merged values.</returns>
    protected virtual IndexValue MergeIndexValue(IndexValue original, IndexValue toMerge) =>
        new()
        {
            Keywords = MergeValues(original.Keywords, toMerge.Keywords),
            Integers = MergeValues(original.Integers, toMerge.Integers),
            Decimals = MergeValues(original.Decimals, toMerge.Decimals),
            DateTimeOffsets = MergeValues(original.DateTimeOffsets, toMerge.DateTimeOffsets),
            Texts = MergeValues(original.Texts, toMerge.Texts),
            TextsR1 = MergeValues(original.TextsR1, toMerge.TextsR1),
            TextsR2 = MergeValues(original.TextsR2, toMerge.TextsR2),
            TextsR3 = MergeValues(original.TextsR3, toMerge.TextsR3),
        };

    /// <summary>
    /// Merges two value collections by concatenating and removing duplicates.
    /// </summary>
    protected static IEnumerable<T>? MergeValues<T>(IEnumerable<T>? one, IEnumerable<T>? other)
    {
        IEnumerable<T>? list = one;
        if (list is null)
        {
            list = other;
        }
        else
        {
            if (other is not null)
            {
                return list.Concat(other).Distinct();
            }
        }

        return list;
    }

    public Task ResetAsync(string indexAlias)
    {
        var physicalName = ResolveWriteTargetName(indexAlias);

        // NOTE: the index might not exist at this point, so don't use GetIndex (it throws an exception for non-existing indexes)
        if (_examineManager.TryGetIndex(physicalName, out IIndex? index) is false)
        {
            return Task.CompletedTask;
        }

        index.CreateIndex();
        return Task.CompletedTask;
    }

    public Task<IndexMetadata> GetMetadataAsync(string indexAlias)
    {
        if (_activeIndexManager.IsRebuilding(indexAlias))
        {
            // During rebuild, report rebuilding status based on the active index's document count
            var activePhysicalName = _activeIndexManager.ResolveActiveIndexName(indexAlias);
            var activeDocCount = 0L;
            if (_examineManager.TryGetIndex(activePhysicalName, out IIndex? activeIndex) && activeIndex is IIndexStats activeStats)
            {
                activeDocCount = activeStats.GetDocumentCount();
            }

            return Task.FromResult(new IndexMetadata(activeDocCount, HealthStatus.Rebuilding, Constants.Provider.Name));
        }

        var physicalName = _activeIndexManager.ResolveActiveIndexName(indexAlias);
        if (_examineManager.TryGetIndex(physicalName, out IIndex? index) is false)
        {
            return Task.FromResult(new IndexMetadata(0, HealthStatus.Unknown, Constants.Provider.Name));
        }

        var documentCount = 0L;

        if (index is IIndexStats stats)
        {
            documentCount = stats.GetDocumentCount();
            if (documentCount == 0L)
            {
                return Task.FromResult(new IndexMetadata(0, HealthStatus.Empty, Constants.Provider.Name));
            }
        }

        // Attempt to query the index to verify it's readable and not corrupted
        try
        {
            index.Searcher.CreateQuery().ManagedQuery("__healthcheck__").Execute(new QueryOptions(0, 1));
            return Task.FromResult(new IndexMetadata(documentCount, HealthStatus.Healthy, Constants.Provider.Name));
        }
        catch
        {
            return Task.FromResult(new IndexMetadata(documentCount, HealthStatus.Corrupted, Constants.Provider.Name));
        }
    }

    private void DeleteSingleDoc(IIndex index, Guid key)
    {
        ISearchResults documents = index.Searcher.CreateQuery().Field(FieldNameHelper.FieldName(CoreConstants.FieldNames.Id, Constants.FieldValues.Keywords), key.AsKeyword()).Execute();

        var idsToDelete = new HashSet<string>();

        foreach (ISearchResult document in documents)
        {
            idsToDelete.Add(document.Id);
        }

        if (idsToDelete.Any())
        {
            index.DeleteFromIndex(idsToDelete);
        }
    }

    public Task DeleteAsync(string indexAlias, IEnumerable<Guid> keys)
    {
        IIndex index = GetWriteTargetIndex(indexAlias);
        var idsToDelete = new HashSet<string>();

        foreach (Guid key in keys)
        {
            ISearchResults documents = index.Searcher.CreateQuery().Field(FieldNameHelper.FieldName(CoreConstants.FieldNames.PathIds, Constants.FieldValues.Keywords), key.AsKeyword()).Execute();
            foreach (ISearchResult document in documents)
            {
                idsToDelete.Add(document.Id);
            }

            index.DeleteFromIndex(idsToDelete);
        }

        return Task.CompletedTask;
    }

    private Dictionary<string, IEnumerable<object>> MapToDictionary(IEnumerable<IndexField> fields, string? culture, IEnumerable<string?> segments, ContentProtection? protection)
    {
        var result = new Dictionary<string, IEnumerable<object>>();

        // Aggregated texts grouped by segment (using empty string for null segment)
        var aggregatedTextsBySegment = new Dictionary<string, List<string>>();
        var aggregatedR1TextsBySegment = new Dictionary<string, List<string>>();
        var aggregatedR2TextsBySegment = new Dictionary<string, List<string>>();
        var aggregatedR3TextsBySegment = new Dictionary<string, List<string>>();

        foreach (IndexField field in fields)
        {
            if (field.Value.Integers?.Any() ?? false)
            {
                var integers = field.Value.Integers.Cast<object>().ToList();
                result.Add(FieldNameHelper.FieldName(field, Constants.FieldValues.Integers), integers);
            }

            if (field.Value.Keywords?.Any() ?? false)
            {
                // add field for keyword filtering (will be indexed as RAW)
                var fieldName = FieldNameHelper.FieldName(field, Constants.FieldValues.Keywords);
                result.Add(fieldName, field.Value.Keywords);
                FieldOptions.Field? fieldConfiguration = FieldOptions.Fields.FirstOrDefault(f
                    => f.PropertyName == field.FieldName && f.FieldValues == FieldValues.Keywords);
                if (fieldConfiguration?.Sortable is true || fieldConfiguration?.Facetable is true)
                {
                    // add extra field for sorting and/or faceting (will be indexed according to configuration)
                    result.Add(FieldNameHelper.QueryableKeywordFieldName(fieldName), field.Value.Keywords);
                }
            }

            if (field.Value.Decimals?.Any() ?? false)
            {
                var decimals = field.Value.Decimals.Cast<object>().ToList();
                result.Add(FieldNameHelper.FieldName(field, Constants.FieldValues.Decimals), decimals);
            }

            if (field.Value.DateTimeOffsets?.Any() ?? false)
            {
                // We have to use DateTime here, as examine facets does not play nice with DatetimeOffsets for now.
                var dates = field.Value.DateTimeOffsets.Select(x => x.DateTime).Cast<object>().ToList();
                result.Add(FieldNameHelper.FieldName(field, Constants.FieldValues.DateTimeOffsets), dates);
            }

            if (field.Value.Texts?.Any() ?? false)
            {
                result.Add(FieldNameHelper.FieldName(field, Constants.FieldValues.Texts), field.Value.Texts);
                AddToAggregatedTexts(aggregatedTextsBySegment, field.Segment, field.Value.Texts);
            }

            if (field.Value.TextsR1?.Any() ?? false)
            {
                result.Add(FieldNameHelper.FieldName(field, Constants.FieldValues.TextsR1), field.Value.TextsR1);
                AddToAggregatedTexts(aggregatedR1TextsBySegment, field.Segment, field.Value.TextsR1);
            }

            if (field.Value.TextsR2?.Any() ?? false)
            {
                result.Add(FieldNameHelper.FieldName(field, Constants.FieldValues.TextsR2), field.Value.TextsR2);
                AddToAggregatedTexts(aggregatedR2TextsBySegment, field.Segment, field.Value.TextsR2);
            }

            if (field.Value.TextsR3?.Any() ?? false)
            {
                result.Add(FieldNameHelper.FieldName(field, Constants.FieldValues.TextsR3), field.Value.TextsR3);
                AddToAggregatedTexts(aggregatedR3TextsBySegment, field.Segment, field.Value.TextsR3);
            }

            AppendCustomIndexValues(field, result);
        }

        // Add segment-specific aggregated text fields
        AddAggregatedTextFields(result, Constants.SystemFields.AggregatedTexts, aggregatedTextsBySegment);
        AddAggregatedTextFields(result, Constants.SystemFields.AggregatedTextsR1, aggregatedR1TextsBySegment);
        AddAggregatedTextFields(result, Constants.SystemFields.AggregatedTextsR2, aggregatedR2TextsBySegment);
        AddAggregatedTextFields(result, Constants.SystemFields.AggregatedTextsR3, aggregatedR3TextsBySegment);

        // Cannot add null values, so we have to just say "none" here, so we can filter on variant / invariant content
        result.Add(Constants.SystemFields.Culture, [culture ?? Constants.Variance.Invariant]);
        IEnumerable<Guid> protectionIds = protection?.AccessIds ?? new List<Guid> {Guid.Empty};
        result.Add(Constants.SystemFields.Protection, protectionIds.Select(x => x.AsKeyword()));

        return result;
    }

    private static void AddToAggregatedTexts(Dictionary<string, List<string>> aggregatedTextsBySegment, string? segment, IEnumerable<string> texts)
    {
        // Use empty string as key for null segment
        var key = segment ?? string.Empty;
        if (aggregatedTextsBySegment.TryGetValue(key, out List<string>? list))
        {
            list.AddRange(texts);
        }
        else
        {
            aggregatedTextsBySegment[key] = texts.ToList();
        }
    }

    private static void AddAggregatedTextFields(Dictionary<string, IEnumerable<object>> result, string baseFieldName, Dictionary<string, List<string>> aggregatedTextsBySegment)
    {
        foreach (KeyValuePair<string, List<string>> aggregatedTexts in aggregatedTextsBySegment)
        {
            if (!aggregatedTexts.Value.Any())
            {
                continue;
            }

            // Empty string key means null segment, use base field name
            var fieldName = string.IsNullOrEmpty(aggregatedTexts.Key)
                ? baseFieldName
                : $"{baseFieldName}_{aggregatedTexts.Key}";

            result.Add(fieldName, aggregatedTexts.Value.ToArray());
        }
    }

    private string ResolveWriteTargetName(string indexAlias)
        => _activeIndexManager.IsRebuilding(indexAlias)
            ? _activeIndexManager.ResolveShadowIndexName(indexAlias)
            : _activeIndexManager.ResolveActiveIndexName(indexAlias);

    private IIndex GetWriteTargetIndex(string indexAlias)
    {
        var physicalName = ResolveWriteTargetName(indexAlias);
        return _examineManager.TryGetIndex(physicalName, out IIndex? index)
            ? index
            : throw new ArgumentException($"The index {physicalName} (for alias {indexAlias}) could not be found.", nameof(indexAlias));
    }
}
