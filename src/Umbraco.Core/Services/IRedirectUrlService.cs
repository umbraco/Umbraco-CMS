using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides operations for managing URL redirects for content items.
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
    Task RegisterAsync(string url, Guid contentKey, string? culture = null);

    /// <summary>
    ///     Deletes all redirect URLs for a given content.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    Task DeleteContentRedirectUrlsAsync(Guid contentKey);

    /// <summary>
    ///     Deletes a redirect URL.
    /// </summary>
    /// <param name="redirectUrl">The redirect URL to delete.</param>
    Task DeleteAsync(IRedirectUrl redirectUrl);

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
    ///     Gets the most recent redirect URLs corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <returns>The most recent redirect URLs corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url);

    /// <summary>
    ///     Gets the most recent redirect URLs corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="culture">The culture of the request.</param>
    /// <returns>The most recent redirect URLs corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url, string? culture);

    /// <summary>
    ///     Gets all redirect URLs for a content item.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <returns>All redirect URLs for the content item.</returns>
    Task<IEnumerable<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey);

    /// <summary>
    ///     Gets paginated redirect URLs for a content item.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>All redirect URLs for the content item.</returns>
    Task<PagedModel<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey, int skip, int take);

    /// <summary>
    ///     Gets all redirect URLs.
    /// </summary>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>A paged model containing the redirect URLs and the total count.</returns>
    Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int skip, int take);

    /// <summary>
    ///     Gets all redirect URLs below a given content item.
    /// </summary>
    /// <param name="rootContentId">The content unique identifier.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>A paged model containing the redirect URLs and the total count.</returns>
    Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int rootContentId, int skip, int take);

    /// <summary>
    ///     Searches for all redirect URLs that contain a given search term in their URL property.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>A paged model containing the matching redirect URLs and the total count.</returns>
    Task<PagedModel<IRedirectUrl>> SearchRedirectUrlsAsync(string searchTerm, int skip, int take);
}
