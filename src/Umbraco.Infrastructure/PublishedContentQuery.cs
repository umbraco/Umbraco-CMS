using System.Collections;
using System.Globalization;
using Examine;
using Examine.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure;

/// <summary>
///     A class used to query for published content, media items
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.IPublishedContentQuery" />
public class PublishedContentQuery : IPublishedContentQuery
{
    private readonly IExamineManager _examineManager;
    private readonly IPublishedContentCache _publishedContent;
    private readonly IPublishedMediaCache _publishedMediaCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private static readonly HashSet<string> _returnedQueryFields =
        new() { ExamineFieldNames.ItemIdFieldName, ExamineFieldNames.CategoryFieldName };

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedContentQuery" /> class.
    /// </summary>
    public PublishedContentQuery(
        IVariationContextAccessor variationContextAccessor,
        IExamineManager examineManager,
        IPublishedContentCache publishedContent,
        IPublishedMediaCache publishedMediaCache)
    {
        _variationContextAccessor = variationContextAccessor ??
                                    throw new ArgumentNullException(nameof(variationContextAccessor));
        _examineManager = examineManager ?? throw new ArgumentNullException(nameof(examineManager));
        _publishedContent = publishedContent;
        _publishedMediaCache = publishedMediaCache;
    }

    #region Convert Helpers

    private static bool ConvertIdObjectToInt(object id, out int intId)
    {
        switch (id)
        {
            case string s:
                return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out intId);

            case int i:
                intId = i;
                return true;

            default:
                intId = default;
                return false;
        }
    }

    private static bool ConvertIdObjectToGuid(object id, out Guid guidId)
    {
        switch (id)
        {
            case string s:
                return Guid.TryParse(s, out guidId);

            case Guid g:
                guidId = g;
                return true;

            default:
                guidId = default;
                return false;
        }
    }

    private static bool ConvertIdObjectToUdi(object id, out Udi? guidId)
    {
        switch (id)
        {
            case string s:
                return UdiParser.TryParse(s, out guidId);

            case Udi u:
                guidId = u;
                return true;

            default:
                guidId = default;
                return false;
        }
    }

    #endregion

    #region Content

    public IPublishedContent? Content(int id)
        => ItemById(id, _publishedContent);

    public IPublishedContent? Content(Guid id)
        => ItemById(id, _publishedContent);

    public IPublishedContent? Content(Udi? id)
    {
        if (!(id is GuidUdi udi))
        {
            return null;
        }

        return ItemById(udi.Guid, _publishedContent);
    }

    public IPublishedContent? Content(object id)
    {
        if (ConvertIdObjectToInt(id, out var intId))
        {
            return Content(intId);
        }

        if (ConvertIdObjectToGuid(id, out Guid guidId))
        {
            return Content(guidId);
        }

        if (ConvertIdObjectToUdi(id, out Udi? udiId))
        {
            return Content(udiId);
        }

        return null;
    }

    public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        => ItemsByIds(_publishedContent, ids);

    public IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids)
        => ItemsByIds(_publishedContent, ids);

    public IEnumerable<IPublishedContent> Content(IEnumerable<object> ids)
        => ids.Select(Content).WhereNotNull();

    public IEnumerable<IPublishedContent> ContentAtRoot()
        => ItemsAtRoot(_publishedContent);

    #endregion

    #region Media

    public IPublishedContent? Media(int id)
        => ItemById(id, _publishedMediaCache);

    public IPublishedContent? Media(Guid id)
        => ItemById(id, _publishedMediaCache);

    public IPublishedContent? Media(Udi? id)
    {
        if (!(id is GuidUdi udi))
        {
            return null;
        }

        return ItemById(udi.Guid, _publishedMediaCache);
    }

    public IPublishedContent? Media(object id)
    {
        if (ConvertIdObjectToInt(id, out var intId))
        {
            return Media(intId);
        }

        if (ConvertIdObjectToGuid(id, out Guid guidId))
        {
            return Media(guidId);
        }

        if (ConvertIdObjectToUdi(id, out Udi? udiId))
        {
            return Media(udiId);
        }

        return null;
    }

    public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        => ItemsByIds(_publishedMediaCache, ids);

    public IEnumerable<IPublishedContent> Media(IEnumerable<object> ids)
        => ids.Select(Media).WhereNotNull();

    public IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids)
        => ItemsByIds(_publishedMediaCache, ids);

    public IEnumerable<IPublishedContent> MediaAtRoot()
        => ItemsAtRoot(_publishedMediaCache);

    #endregion

    #region Used by Content/Media

    private static IPublishedContent? ItemById(int id, IPublishedCache? cache)
        => cache?.GetById(id);

    private static IPublishedContent? ItemById(Guid id, IPublishedCache? cache)
        => cache?.GetById(id);

    private static IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache? cache, IEnumerable<int> ids)
        => ids.Select(eachId => ItemById(eachId, cache)).WhereNotNull();

    private IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache? cache, IEnumerable<Guid> ids)
        => ids.Select(eachId => ItemById(eachId, cache)).WhereNotNull();

    private static IEnumerable<IPublishedContent> ItemsAtRoot(IPublishedCache? cache)
        => cache?.GetAtRoot() ?? Array.Empty<IPublishedContent>();

    #endregion

    #region Search

    /// <inheritdoc />
    public IEnumerable<PublishedSearchResult> Search(string term, string culture = "*",
        string indexName = Constants.UmbracoIndexes.ExternalIndexName)
        => Search(term, 0, 0, out _, culture, indexName);

    /// <inheritdoc />
    public IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords,
        string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName,
        ISet<string>? loadedFields = null)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), skip,
                "The value must be greater than or equal to zero.");
        }

        if (take < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take,
                "The value must be greater than or equal to zero.");
        }

        if (string.IsNullOrEmpty(indexName))
        {
            indexName = Constants.UmbracoIndexes.ExternalIndexName;
        }

        if (!_examineManager.TryGetIndex(indexName, out IIndex? index) || index is not IUmbracoIndex umbIndex)
        {
            throw new InvalidOperationException(
                $"No index found by name {indexName} or is not of type {typeof(IUmbracoIndex)}");
        }

        IQuery? query = umbIndex.Searcher.CreateQuery(IndexTypes.Content);

        IOrdering ordering;
        if (culture == "*")
        {
            // Search everything
            ordering = query.ManagedQuery(term);
        }
        else if (string.IsNullOrWhiteSpace(culture))
        {
            // Only search invariant
            ordering = query
                .Field(UmbracoExamineFieldNames.VariesByCultureFieldName, "n") // Must not vary by culture
                .And().ManagedQuery(term);
        }
        else
        {
            // Only search the specified culture
            var fields =
                umbIndex.GetCultureAndInvariantFields(culture)
                    .ToArray(); // Get all index fields suffixed with the culture name supplied

            // Filter out unpublished content for the specified culture if the content varies by culture
            // The published__{culture} field is not populated when the content is not published in that culture
            ordering = query
                .ManagedQuery(term, fields)
                .Not().Group(q => q
                    .Field(UmbracoExamineFieldNames.VariesByCultureFieldName, "y")
                    .Not().Field($"{UmbracoExamineFieldNames.PublishedFieldName}_{culture.ToLowerInvariant()}", "y"));
        }

        return Search(ordering, skip, take, out totalRecords, culture);
    }

    /// <inheritdoc />
    public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query)
        => Search(query, 0, 0, out _);

    /// <inheritdoc />
    public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take, out long totalRecords)
        => Search(query, skip, take, out totalRecords, null);

    /// <inheritdoc />
    public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take, out long totalRecords, string? culture)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), skip,
                "The value must be greater than or equal to zero.");
        }

        if (take < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take,
                "The value must be greater than or equal to zero.");
        }

        if (query is IOrdering ordering)
        {
            // Filter selected fields because results are loaded from the published snapshot based on these
            query = ordering.SelectFields(_returnedQueryFields);
        }

        ISearchResults? results = skip == 0 && take == 0
            ? query.Execute()
            : query.Execute(QueryOptions.SkipTake(skip, take));

        totalRecords = results.TotalItemCount;

        return culture.IsNullOrWhiteSpace()
            ? results.ToPublishedSearchResults(_publishedContent)
            : new CultureContextualSearchResults(results.ToPublishedSearchResults(_publishedContent), _variationContextAccessor, culture);
    }

    /// <summary>
    ///     This is used to contextualize the values in the search results when enumerating over them, so that the correct
    ///     culture values are used.
    /// </summary>
    private sealed class CultureContextualSearchResults : IEnumerable<PublishedSearchResult>
    {
        private readonly string _culture;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IEnumerable<PublishedSearchResult> _wrapped;

        public CultureContextualSearchResults(
            IEnumerable<PublishedSearchResult> wrapped,
            IVariationContextAccessor variationContextAccessor,
            string culture)
        {
            _wrapped = wrapped;
            _variationContextAccessor = variationContextAccessor;
            _culture = culture;
        }

        public IEnumerator<PublishedSearchResult> GetEnumerator()
        {
            // We need to change the current culture to what is requested and then change it back
            VariationContext? originalContext = _variationContextAccessor.VariationContext;
            if (!_culture.IsNullOrWhiteSpace() && !_culture.InvariantEquals(originalContext?.Culture))
            {
                _variationContextAccessor.VariationContext = new VariationContext(_culture);
            }

            // Now the IPublishedContent returned will be contextualized to the culture specified and will be reset when the enumerator is disposed
            return new CultureContextualSearchResultsEnumerator(_wrapped.GetEnumerator(), _variationContextAccessor,
                originalContext);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Resets the variation context when this is disposed.
        /// </summary>
        private sealed class CultureContextualSearchResultsEnumerator : IEnumerator<PublishedSearchResult>
        {
            private readonly VariationContext? _originalContext;
            private readonly IVariationContextAccessor _variationContextAccessor;
            private readonly IEnumerator<PublishedSearchResult> _wrapped;

            public CultureContextualSearchResultsEnumerator(
                IEnumerator<PublishedSearchResult> wrapped,
                IVariationContextAccessor variationContextAccessor,
                VariationContext? originalContext)
            {
                _wrapped = wrapped;
                _variationContextAccessor = variationContextAccessor;
                _originalContext = originalContext;
            }

            public PublishedSearchResult Current => _wrapped.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _wrapped.Dispose();

                // Reset to original variation context
                _variationContextAccessor.VariationContext = _originalContext;
            }

            public bool MoveNext() => _wrapped.MoveNext();

            public void Reset() => _wrapped.Reset();
        }
    }

    #endregion
}
