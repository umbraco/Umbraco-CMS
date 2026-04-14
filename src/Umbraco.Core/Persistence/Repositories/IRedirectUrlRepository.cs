using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the <see cref="IRedirectUrl" /> repository.
/// </summary>
public interface IRedirectUrlRepository : IAsyncReadWriteRepository<Guid, IRedirectUrl>
{
    /// <summary>
    ///     Gets a redirect URL.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="contentKey">The content unique key.</param>
    /// <param name="culture">The culture.</param>
    Task<IRedirectUrl?> GetAsync(string url, Guid contentKey, string? culture);

    /// <summary>
    ///     Deletes a redirect URL.
    /// </summary>
    /// <param name="id">The redirect URL identifier.</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    ///     Deletes all redirect URLs.
    /// </summary>
    Task DeleteAllAsync();

    /// <summary>
    ///     Deletes all redirect URLs for a given content.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    Task DeleteContentUrlsAsync(Guid contentKey);

    /// <summary>
    ///     Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentUrlAsync(string url);

    /// <summary>
    /// Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="culture">The culture the domain is associated with</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentUrlAsync(string url, string culture);

    /// <summary>
    ///     Gets all redirect URLs for a content item.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <returns>All redirect URLs for the content item.</returns>
    Task<IEnumerable<IRedirectUrl>> GetContentUrlsAsync(Guid contentKey);

    /// <summary>
    ///     Gets all redirect URLs.
    /// </summary>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paged model containing the redirect URLs and the total count.</returns>
    Task<PagedModel<IRedirectUrl>> GetAllUrlsAsync(long pageIndex, int pageSize);

    /// <summary>
    ///     Gets all redirect URLs below a given content item.
    /// </summary>
    /// <param name="rootContentId">The content unique identifier.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paged model containing the redirect URLs and the total count.</returns>
    Task<PagedModel<IRedirectUrl>> GetAllUrlsAsync(int rootContentId, long pageIndex, int pageSize);

    /// <summary>
    ///     Searches for all redirect URLs that contain a given search term in their URL property.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paged model containing the matching redirect URLs and the total count.</returns>
    Task<PagedModel<IRedirectUrl>> SearchUrlsAsync(string searchTerm, long pageIndex, int pageSize);
}
