using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// </summary>
public interface IRedirectUrlService : IService
{
    /// <summary>
    ///     Registers a redirect URL.
    /// </summary>
    /// <param name="url">The Umbraco URL route.</param>
    /// <param name="contentKey">The content unique key.</param>
    /// <param name="culture">The culture.</param>
    /// <remarks>Is a proper Umbraco route eg /path/to/foo or 123/path/tofoo.</remarks>
    void Register(string url, Guid contentKey, string? culture = null);

    /// <summary>
    ///     Deletes all redirect URLs for a given content.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    void DeleteContentRedirectUrls(Guid contentKey);

    /// <summary>
    ///     Deletes a redirect URL.
    /// </summary>
    /// <param name="redirectUrl">The redirect URL to delete.</param>
    void Delete(IRedirectUrl redirectUrl);

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
    ///     Gets the most recent redirect URLs corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <returns>The most recent redirect URLs corresponding to the route.</returns>
    IRedirectUrl? GetMostRecentRedirectUrl(string url);

    /// <summary>
    ///     Gets the most recent redirect URLs corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="culture">The culture of the request.</param>
    /// <returns>The most recent redirect URLs corresponding to the route.</returns>
    IRedirectUrl? GetMostRecentRedirectUrl(string url, string? culture);

    /// <summary>
    /// Gets the most recent redirect URLs corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="culture">The culture of the request.</param>
    /// <returns>The most recent redirect URLs corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url, string? culture) => Task.FromResult(GetMostRecentRedirectUrl(url, culture));

        /// <summary>
    ///     Gets all redirect URLs for a content item.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <returns>All redirect URLs for the content item.</returns>
    IEnumerable<IRedirectUrl> GetContentRedirectUrls(Guid contentKey);

    /// <summary>
    ///     Gets all redirect URLs.
    /// </summary>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> GetAllRedirectUrls(long pageIndex, int pageSize, out long total);

    /// <summary>
    ///     Gets all redirect URLs below a given content item.
    /// </summary>
    /// <param name="rootContentId">The content unique identifier.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> GetAllRedirectUrls(int rootContentId, long pageIndex, int pageSize, out long total);

    /// <summary>
    ///     Searches for all redirect URLs that contain a given search term in their URL property.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> SearchRedirectUrls(string searchTerm, long pageIndex, int pageSize, out long total);
}
