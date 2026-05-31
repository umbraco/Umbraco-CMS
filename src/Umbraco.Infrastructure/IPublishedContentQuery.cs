using Examine.Search;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core;

/// <summary>
///     Query methods used for accessing strongly typed content in templates.
/// </summary>
public interface IPublishedContentQuery
{
    /// <summary>
    /// Retrieves the published content item corresponding to the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the content item to retrieve.</param>
    /// <returns>The <see cref="IPublishedContent"/> instance if found; otherwise, <c>null</c>.</returns>
    IPublishedContent? Content(int id);

    /// <summary>
    /// Returns the published content item with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique <see cref="Guid"/> identifier of the content item.</param>
    /// <returns>The <see cref="IPublishedContent"/> instance if found; otherwise, <c>null</c>.</returns>
    IPublishedContent? Content(Guid id);

    /// <summary>
    /// Retrieves the published content item with the specified unique document identifier (UDI).
    /// </summary>
    /// <param name="id">The unique document identifier (UDI) of the content item to retrieve.</param>
    /// <returns>The <see cref="IPublishedContent"/> instance if found; otherwise, <c>null</c>.</returns>
    IPublishedContent? Content(Udi id);

    /// <summary>
    /// Gets the published content item with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the content item to retrieve.</param>
    /// <returns>The published content item if found; otherwise, null.</returns>
    IPublishedContent? Content(object id);

    /// <summary>
    /// Retrieves the published content items corresponding to the specified collection of content IDs.
    /// </summary>
    /// <param name="ids">A collection of content item IDs to look up.</param>
    /// <returns>An enumerable of <see cref="IPublishedContent"/> instances matching the provided IDs. Items not found are omitted.</returns>
    IEnumerable<IPublishedContent> Content(IEnumerable<int> ids);

    /// <summary>Gets the published content items for the specified unique identifiers.</summary>
    /// <param name="ids">The unique identifiers of the content items to retrieve.</param>
    /// <returns>An enumerable collection of published content items matching the specified identifiers.</returns>
    IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids);

    /// <summary>
    /// Retrieves the published content items corresponding to the specified IDs.
    /// </summary>
    /// <param name="ids">A collection of IDs identifying the content items to retrieve.</param>
    /// <returns>An enumerable collection of <see cref="IPublishedContent"/> items matching the provided IDs.</returns>
    IEnumerable<IPublishedContent> Content(IEnumerable<object> ids);

    /// <summary>Gets the published content items at the root level of the content tree.</summary>
    /// <returns>An enumerable collection of root-level <see cref="Umbraco.Cms.Core.Models.IPublishedContent"/> items.</returns>
    IEnumerable<IPublishedContent> ContentAtRoot();

    /// <summary>
    /// Gets the media item with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the media item.</param>
    /// <returns>The media item if found; otherwise, null.</returns>
    IPublishedContent? Media(int id);

    /// <summary>
    /// Returns the media item with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique <see cref="Guid"/> identifier of the media item.</param>
    /// <returns>The <see cref="IPublishedContent"/> representing the media item if found; otherwise, <c>null</c>.</returns>
    IPublishedContent? Media(Guid id);

    /// <summary>
    /// Retrieves a media item using its unique identifier (UDI).
    /// </summary>
    /// <param name="id">The unique identifier (UDI) of the media item.</param>
    /// <returns>The <see cref="IPublishedContent"/> representing the media item if found; otherwise, <c>null</c>.</returns>
    IPublishedContent? Media(Udi id);

    /// <summary>
    /// Returns the media item corresponding to the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the media item to retrieve.</param>
    /// <returns>The <see cref="IPublishedContent"/> representing the media item if found; otherwise, <c>null</c>.</returns>
    IPublishedContent? Media(object id);

    /// <summary>Gets the media items corresponding to the specified IDs.</summary>
    /// <param name="ids">The collection of media item IDs to retrieve.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Umbraco.Cms.Core.Models.IPublishedContent"/> representing the found media items.</returns>
    IEnumerable<IPublishedContent> Media(IEnumerable<int> ids);

    /// <summary>
    /// Retrieves media items corresponding to the specified identifiers.
    /// </summary>
    /// <param name="ids">A collection of identifiers for the media items to retrieve.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Umbraco.Cms.Core.Models.IPublishedContent"/> representing the found media items.</returns>
    IEnumerable<IPublishedContent> Media(IEnumerable<object> ids);

    /// <summary>
    /// Retrieves media items corresponding to the specified unique identifiers.
    /// </summary>
    /// <param name="ids">A collection of unique identifiers for the media items to retrieve.</param>
    /// <returns>An enumerable collection of media items matching the provided identifiers.</returns>
    IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids);

    /// <summary>
    /// Returns all media items that are located at the root of the media library.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{IPublishedContent}"/> containing the root-level media items.</returns>
    IEnumerable<IPublishedContent> MediaAtRoot();

    /// <summary>
    ///     Searches content.
    /// </summary>
    /// <param name="term">The term to search.</param>
    /// <param name="skip">The amount of results to skip.</param>
    /// <param name="take">The amount of results to take/return.</param>
    /// <param name="totalRecords">The total amount of records.</param>
    /// <param name="culture">The culture (defaults to a culture insensitive search).</param>
    /// <param name="indexName">
    ///     The name of the index to search (defaults to
    ///     <see cref="Constants.UmbracoIndexes.ExternalIndexName" />).
    /// </param>
    /// <param name="loadedFields">
    ///     This parameter is no longer used, because the results are loaded from the published snapshot
    ///     using the single item ID field.
    /// </param>
    /// <returns>
    ///     The search results.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         When the <paramref name="culture" /> is not specified or is *, all cultures are searched.
    ///         To search for only invariant documents and fields use null.
    ///         When searching on a specific culture, all culture specific fields are searched for the provided culture and all
    ///         invariant fields for all documents.
    ///     </para>
    ///     <para>While enumerating results, the ambient culture is changed to be the searched culture.</para>
    /// </remarks>
    IEnumerable<PublishedSearchResult> Search(
        string term,
        int skip,
        int take,
        out long totalRecords,
        string culture = "*",
        string indexName = Constants.UmbracoIndexes.ExternalIndexName,
        ISet<string>? loadedFields = null);

    /// <summary>
    ///     Searches content.
    /// </summary>
    /// <param name="term">The term to search.</param>
    /// <param name="culture">The culture (defaults to a culture insensitive search).</param>
    /// <param name="indexName">
    ///     The name of the index to search (defaults to
    ///     <see cref="Cms.Core.Constants.UmbracoIndexes.ExternalIndexName" />).
    /// </param>
    /// <returns>
    ///     The search results.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         When the <paramref name="culture" /> is not specified or is *, all cultures are searched.
    ///         To search for only invariant documents and fields use null.
    ///         When searching on a specific culture, all culture specific fields are searched for the provided culture and all
    ///         invariant fields for all documents.
    ///     </para>
    ///     <para>While enumerating results, the ambient culture is changed to be the searched culture.</para>
    /// </remarks>
    IEnumerable<PublishedSearchResult> Search(string term, string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName);

    /// <summary>
    ///     Executes the query and converts the results to <see cref="PublishedSearchResult" />.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>
    ///     The search results.
    /// </returns>
    IEnumerable<PublishedSearchResult> Search(IQueryExecutor query);

    /// <summary>
    ///     Executes the query and converts the results to <see cref="PublishedSearchResult" />.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="skip">The amount of results to skip.</param>
    /// <param name="take">The amount of results to take/return.</param>
    /// <param name="totalRecords">The total amount of records.</param>
    /// <returns>
    ///     The search results.
    /// </returns>
    IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take, out long totalRecords);

    /// <summary>
    ///     Executes the query and converts the results to <see cref="PublishedSearchResult" />.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="skip">The amount of results to skip.</param>
    /// <param name="take">The amount of results to take/return.</param>
    /// <param name="totalRecords">The total amount of records.</param>
    /// <param name="culture">The culture (defaults to a culture insensitive search).</param>
    /// <returns>
    ///     The search results.
    /// </returns>
    /// <remarks>
    ///     While enumerating results, the ambient culture is changed to be the searched culture.
    /// </remarks>
    IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take, out long totalRecords, string? culture)
        => Search(query, skip, take, out totalRecords);
}
