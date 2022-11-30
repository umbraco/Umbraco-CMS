using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the <see cref="IRedirectUrl" /> repository.
/// </summary>
public interface IRedirectUrlRepository : IReadWriteQueryRepository<Guid, IRedirectUrl>
{
    /// <summary>
    ///     Gets a redirect URL.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="contentKey">The content unique key.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    IRedirectUrl? Get(string url, Guid contentKey, string? culture);

    /// <summary>
    ///     Deletes a redirect URL.
    /// </summary>
    /// <param name="id">The redirect URL identifier.</param>
    void Delete(Guid id);

    /// <summary>
    ///     Deletes all redirect URLs.
    /// </summary>
    void DeleteAll();

    /// <summary>
    ///     Deletes all redirect URLs for a given content.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    void DeleteContentUrls(Guid contentKey);

    /// <summary>
    ///     Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    IRedirectUrl? GetMostRecentUrl(string url);

    /// <summary>
    ///     Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentUrlAsync(string url) => Task.FromResult(GetMostRecentUrl(url));

    /// <summary>
    /// Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="culture">The culture the domain is associated with</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    IRedirectUrl? GetMostRecentUrl(string url, string culture);

    /// <summary>
    /// Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="culture">The culture the domain is associated with</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentUrlAsync(string url, string culture) => Task.FromResult(GetMostRecentUrl(url, culture));

    /// <summary>
    ///     Gets all redirect URLs for a content item.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <returns>All redirect URLs for the content item.</returns>
    IEnumerable<IRedirectUrl> GetContentUrls(Guid contentKey);

    /// <summary>
    ///     Gets all redirect URLs.
    /// </summary>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total);

    /// <summary>
    ///     Gets all redirect URLs below a given content item.
    /// </summary>
    /// <param name="rootContentId">The content unique identifier.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total);

    /// <summary>
    ///     Searches for all redirect URLs that contain a given search term in their URL property.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> SearchUrls(string searchTerm, long pageIndex, int pageSize, out long total);
}
