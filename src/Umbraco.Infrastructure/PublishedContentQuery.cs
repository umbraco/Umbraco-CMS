using System.Collections;
using System.Globalization;
using Examine;
using Examine.Search;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure;

/// <summary>
///     A class used to query for published content and media items.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.IPublishedContentQuery" />
public class PublishedContentQuery : IPublishedContentQuery
{
    private readonly IExamineManager _examineManager;
    private readonly IPublishedContentCache _publishedContent;
    private readonly IPublishedMediaCache _publishedMediaCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;

    private static readonly HashSet<string> _returnedQueryFields =
        new() { ExamineFieldNames.ItemIdFieldName, ExamineFieldNames.CategoryFieldName };

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedContentQuery" /> class.
    /// </summary>
    public PublishedContentQuery(
        IVariationContextAccessor variationContextAccessor,
        IExamineManager examineManager,
        IPublishedContentCache publishedContent,
        IPublishedMediaCache publishedMediaCache,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IMediaNavigationQueryService mediaNavigationQueryService)
    {
        _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
        _examineManager = examineManager ?? throw new ArgumentNullException(nameof(examineManager));
        _publishedContent = publishedContent;
        _publishedMediaCache = publishedMediaCache;
        _documentNavigationQueryService = documentNavigationQueryService;
        _mediaNavigationQueryService = mediaNavigationQueryService;
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public PublishedContentQuery(
        IVariationContextAccessor variationContextAccessor,
        IExamineManager examineManager,
        IPublishedContentCache publishedContent,
        IPublishedMediaCache publishedMediaCache,
        IDocumentNavigationQueryService documentNavigationQueryService)
        : this(
            variationContextAccessor,
            examineManager,
            publishedContent,
            publishedMediaCache,
            documentNavigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>())
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public PublishedContentQuery(
        IVariationContextAccessor variationContextAccessor,
        IExamineManager examineManager,
        IPublishedContentCache publishedContent,
        IPublishedMediaCache publishedMediaCache)
        : this(
            variationContextAccessor,
            examineManager,
            publishedContent,
            publishedMediaCache,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>())
    {
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

    /// <summary>Gets the published content item by its identifier.</summary>
    /// <param name="id">The identifier of the content item.</param>
    /// <returns>The published content item if found; otherwise, null.</returns>
    public IPublishedContent? Content(int id)
        => ItemById(id, _publishedContent);

    /// <summary>Gets the published content item by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the content item.</param>
    /// <returns>The published content item if found; otherwise, <c>null</c>.</returns>
    public IPublishedContent? Content(Guid id)
        => ItemById(id, _publishedContent);

    /// <summary>
    /// Retrieves the published content item corresponding to the specified unique document identifier (UDI).
    /// </summary>
    /// <param name="id">The unique document identifier (UDI) of the content item to retrieve. May be <c>null</c>.</param>
    /// <returns>The published content item if found; otherwise, <c>null</c>.</returns>
    public IPublishedContent? Content(Udi? id)
    {
        if (!(id is GuidUdi udi))
        {
            return null;
        }

        return ItemById(udi.Guid, _publishedContent);
    }

    /// <summary>
    /// Gets the published content item with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the content item. Can be an <see cref="int"/> or another type convertible to an integer.</param>
    /// <returns>The published content item if found; otherwise, <c>null</c>.</returns>
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

    /// <summary>
    /// Gets the published content items for the specified collection of IDs.
    /// </summary>
    /// <param name="ids">The collection of content IDs to retrieve.</param>
    /// <returns>An enumerable of published content items matching the given IDs.</returns>
    public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        => ItemsByIds(_publishedContent, ids);

    /// <summary>Gets the published content items corresponding to the specified unique identifiers.</summary>
    /// <param name="ids">The unique identifiers of the content items to retrieve.</param>
    /// <returns>An enumerable of published content items matching the specified IDs.</returns>
    public IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids)
        => ItemsByIds(_publishedContent, ids);

    /// <summary>
    /// Retrieves the published content items corresponding to the specified identifiers.
    /// </summary>
    /// <param name="ids">A collection of identifiers for the content items to retrieve.</param>
    /// <returns>An enumerable collection of published content items that match the provided identifiers.</returns>
    public IEnumerable<IPublishedContent> Content(IEnumerable<object> ids)
        => ids.Select(Content).WhereNotNull();

    /// <summary>
    /// Returns the published content items located at the root of the content tree.
    /// </summary>
    /// <returns>An enumerable collection of root-level published content items.</returns>
    public IEnumerable<IPublishedContent> ContentAtRoot()
        => ItemsAtRoot(_publishedContent, _documentNavigationQueryService);

    #endregion

    #region Media

    /// <summary>Gets the media item with the specified identifier.</summary>
    /// <param name="id">The identifier of the media item.</param>
    /// <returns>The media item if found; otherwise, <c>null</c>.</returns>
    public IPublishedContent? Media(int id)
        => ItemById(id, _publishedMediaCache);

    /// <summary>Gets media content by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the media content.</param>
    /// <returns>The media content if found; otherwise, null.</returns>
    public IPublishedContent? Media(Guid id)
        => ItemById(id, _publishedMediaCache);

    /// <summary>
    /// Gets a media item by its unique identifier (Udi).
    /// </summary>
    /// <param name="id">The unique identifier (<see cref="Udi"/>) of the media item. If null, returns null.</param>
    /// <returns>The <see cref="IPublishedContent"/> representing the media item if found; otherwise, <c>null</c>.</returns>
    public IPublishedContent? Media(Udi? id)
    {
        if (!(id is GuidUdi udi))
        {
            return null;
        }

        return ItemById(udi.Guid, _publishedMediaCache);
    }

    /// <summary>Gets a media item by its identifier.</summary>
    /// <param name="id">The identifier of the media item.</param>
    /// <returns>The media item if found; otherwise, null.</returns>
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

    /// <summary>
    /// Retrieves the media items corresponding to the specified IDs.
    /// </summary>
    /// <param name="ids">A collection of media item IDs to retrieve.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Umbraco.Cms.Core.Models.IPublishedContent"/> representing the found media items.</returns>
    public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        => ItemsByIds(_publishedMediaCache, ids);

    /// <summary>
    /// Retrieves a collection of media items corresponding to the specified identifiers.
    /// </summary>
    /// <param name="ids">A collection of identifiers for the media items to retrieve.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Umbraco.Cms.Core.Models.IPublishedContent"/> representing the found media items.</returns>
    public IEnumerable<IPublishedContent> Media(IEnumerable<object> ids)
        => ids.Select(Media).WhereNotNull();

    /// <summary>
    /// Retrieves the published media items corresponding to the specified unique identifiers.
    /// </summary>
    /// <param name="ids">A collection of unique identifiers (GUIDs) for the media items to retrieve.</param>
    /// <returns>An <see cref="IEnumerable{IPublishedContent}"/> containing the published media items that match the provided identifiers.</returns>
    public IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids)
        => ItemsByIds(_publishedMediaCache, ids);

    /// <summary>
    /// Returns all media items that are at the root level of the media library.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{IPublishedContent}"/> containing the root-level media items.</returns>
    public IEnumerable<IPublishedContent> MediaAtRoot()
        => ItemsAtRoot(_publishedMediaCache, _mediaNavigationQueryService);

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

    private static IEnumerable<IPublishedContent> ItemsAtRoot(IPublishedCache? cache, INavigationQueryService navigationQueryService)
        => navigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false ? []
             : rootKeys.Select(x => cache?.GetById(false, x)).WhereNotNull();

    #endregion

    #region Search

    /// <inheritdoc />
    public IEnumerable<PublishedSearchResult> Search(
        string term,
        string culture = "*",
        string indexName = Constants.UmbracoIndexes.ExternalIndexName)
        => Search(term, 0, 0, out _, culture, indexName);

    /// <inheritdoc />
    public IEnumerable<PublishedSearchResult> Search(
        string term,
        int skip,
        int take,
        out long totalRecords,
        string culture = "*",
        string indexName = Constants.UmbracoIndexes.ExternalIndexName,
        ISet<string>? loadedFields = null)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(skip),
                skip,
                "The value must be greater than or equal to zero.");
        }

        if (take < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                take,
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
            throw new ArgumentOutOfRangeException(
                nameof(skip),
                skip,
                "The value must be greater than or equal to zero.");
        }

        if (take < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                take,
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

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureContextualSearchResults"/> class, wrapping a collection of search results and contextualizing them for a specific culture.
        /// </summary>
        /// <param name="wrapped">The collection of <see cref="PublishedSearchResult"/> items to wrap.</param>
        /// <param name="variationContextAccessor">The accessor used to obtain variation context information.</param>
        /// <param name="culture">The culture identifier to contextualize the search results for (e.g., "en-US").</param>
        public CultureContextualSearchResults(
            IEnumerable<PublishedSearchResult> wrapped,
            IVariationContextAccessor variationContextAccessor,
            string culture)
        {
            _wrapped = wrapped;
            _variationContextAccessor = variationContextAccessor;
            _culture = culture;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the search results, contextualized to the specified culture.
        /// </summary>
        /// <returns>
        /// An enumerator of <see cref="Umbraco.Cms.Core.Models.PublishedContent.PublishedSearchResult"/> objects, where each result is contextualized to the specified culture for the duration of enumeration.
        /// </returns>
        public IEnumerator<PublishedSearchResult> GetEnumerator()
        {
            // We need to change the current culture to what is requested and then change it back
            VariationContext? originalContext = _variationContextAccessor.VariationContext;
            if (!_culture.IsNullOrWhiteSpace() && !_culture.InvariantEquals(originalContext?.Culture))
            {
                _variationContextAccessor.VariationContext = new VariationContext(_culture);
            }

            // Now the IPublishedContent returned will be contextualized to the culture specified and will be reset when the enumerator is disposed
            return new CultureContextualSearchResultsEnumerator(
                _wrapped.GetEnumerator(),
                _variationContextAccessor,
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

            /// <summary>
            /// Initializes a new instance of the <see cref="CultureContextualSearchResultsEnumerator"/> class.
            /// </summary>
            /// <param name="wrapped">The enumerator of <see cref="PublishedSearchResult"/> items to wrap.</param>
            /// <param name="variationContextAccessor">The accessor used to manage culture variation context during enumeration.</param>
            /// <param name="originalContext">The original variation context, or <c>null</c> if none was provided.</param>
            public CultureContextualSearchResultsEnumerator(
                IEnumerator<PublishedSearchResult> wrapped,
                IVariationContextAccessor variationContextAccessor,
                VariationContext? originalContext)
            {
                _wrapped = wrapped;
                _variationContextAccessor = variationContextAccessor;
                _originalContext = originalContext;
            }

            /// <summary>Gets the current <see cref="PublishedSearchResult"/> in the enumerator.</summary>
            public PublishedSearchResult Current => _wrapped.Current;

            object IEnumerator.Current => Current;

            /// <summary>
            /// Releases all resources used by the enumerator and restores the original variation context.
            /// This ensures that any changes to the variation context during enumeration are reverted.
            /// </summary>
            public void Dispose()
            {
                _wrapped.Dispose();

                // Reset to original variation context
                _variationContextAccessor.VariationContext = _originalContext;
            }

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
            /// <returns><c>true</c> if the enumerator was successfully advanced to the next element; <c>false</c> if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext() => _wrapped.MoveNext();

            /// <summary>
            /// Resets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => _wrapped.Reset();
        }
    }

    #endregion
}
