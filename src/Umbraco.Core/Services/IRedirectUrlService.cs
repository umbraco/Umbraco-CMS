using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

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
    [Obsolete("Use the Register overload that takes the new URL so notification handlers receive full context. Scheduled for removal in Umbraco 20.")]
    void Register(string url, Guid contentKey, string? culture = null);

    /// <summary>
    ///     Registers a redirect URL.
    /// </summary>
    /// <param name="oldUrl">The previous Umbraco URL route the redirect is being created from.</param>
    /// <param name="newUrl">The current Umbraco URL route the redirect is being created to.</param>
    /// <param name="contentKey">The content unique key.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}" /> containing the registered redirect URL on success, or
    ///     <see cref="RedirectUrlOperationStatus.CancelledByNotification" /> if a notification handler
    ///     canceled the operation.
    /// </returns>
    // TODO (V20): Remove the default implementation when the obsolete Register overload is removed.
    Attempt<IRedirectUrl?, RedirectUrlOperationStatus> Register(string oldUrl, string? newUrl, Guid contentKey, string? culture = null)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Register(oldUrl, contentKey, culture);
#pragma warning restore CS0618 // Type or member is obsolete
        return Attempt.SucceedWithStatus<IRedirectUrl?, RedirectUrlOperationStatus>(RedirectUrlOperationStatus.Success, null);
    }

    /// <summary>
    ///     Deletes all redirect URLs for a given content.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    [Obsolete("Use DeleteContentRedirectUrlsWithStatus to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
    void DeleteContentRedirectUrls(Guid contentKey);

    /// <summary>
    ///     Deletes all redirect URLs for a given content, returning the operation status.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <returns>
    ///     <see cref="RedirectUrlOperationStatus.Success" /> on success, or
    ///     <see cref="RedirectUrlOperationStatus.CancelledByNotification" /> if a notification handler
    ///     canceled the operation.
    /// </returns>
    // TODO (V20): Remove the default implementation when the obsolete DeleteContentRedirectUrls overload is removed.
    RedirectUrlOperationStatus DeleteContentRedirectUrlsWithStatus(Guid contentKey)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        DeleteContentRedirectUrls(contentKey);
#pragma warning restore CS0618 // Type or member is obsolete
        return RedirectUrlOperationStatus.Success;
    }

    /// <summary>
    ///     Deletes a redirect URL.
    /// </summary>
    /// <param name="redirectUrl">The redirect URL to delete.</param>
    [Obsolete("Use DeleteWithStatus(IRedirectUrl) to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
    void Delete(IRedirectUrl redirectUrl);

    /// <summary>
    ///     Deletes a redirect URL, returning the operation status.
    /// </summary>
    /// <param name="redirectUrl">The redirect URL to delete.</param>
    /// <returns>
    ///     <see cref="RedirectUrlOperationStatus.Success" /> on success, or
    ///     <see cref="RedirectUrlOperationStatus.CancelledByNotification" /> if a notification handler
    ///     canceled the operation.
    /// </returns>
    // TODO (V20): Remove the default implementation when the obsolete Delete(IRedirectUrl) overload is removed.
    RedirectUrlOperationStatus DeleteWithStatus(IRedirectUrl redirectUrl)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Delete(redirectUrl);
#pragma warning restore CS0618 // Type or member is obsolete
        return RedirectUrlOperationStatus.Success;
    }

    /// <summary>
    ///     Deletes a redirect URL.
    /// </summary>
    /// <param name="id">The redirect URL identifier.</param>
    [Obsolete("Use DeleteWithStatus(Guid) to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
    void Delete(Guid id);

    /// <summary>
    ///     Deletes a redirect URL by its identifier, returning the operation status.
    /// </summary>
    /// <param name="id">The redirect URL identifier.</param>
    /// <returns>
    ///     <see cref="RedirectUrlOperationStatus.Success" /> on success,
    ///     <see cref="RedirectUrlOperationStatus.NotFound" /> if no redirect URL with the given identifier exists, or
    ///     <see cref="RedirectUrlOperationStatus.CancelledByNotification" /> if a notification handler
    ///     canceled the operation.
    /// </returns>
    // TODO (V20): Remove the default implementation when the obsolete Delete(Guid) overload is removed.
    RedirectUrlOperationStatus DeleteWithStatus(Guid id)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Delete(id);
#pragma warning restore CS0618 // Type or member is obsolete
        return RedirectUrlOperationStatus.Success;
    }

    /// <summary>
    ///     Deletes all redirect URLs.
    /// </summary>
    [Obsolete("Use DeleteAllWithStatus to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
    void DeleteAll();

    /// <summary>
    ///     Deletes all redirect URLs, returning the operation status.
    /// </summary>
    /// <returns>
    ///     <see cref="RedirectUrlOperationStatus.Success" /> on success, or
    ///     <see cref="RedirectUrlOperationStatus.CancelledByNotification" /> if a notification handler
    ///     canceled the operation.
    /// </returns>
    // TODO (V20): Remove the default implementation when the obsolete DeleteAll overload is removed.
    RedirectUrlOperationStatus DeleteAllWithStatus()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        DeleteAll();
#pragma warning restore CS0618 // Type or member is obsolete
        return RedirectUrlOperationStatus.Success;
    }

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
    ///     Gets all redirect URLs.
    /// </summary>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> GetAllRedirectUrls(int skip, int take, out long total)
    {
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        return GetAllRedirectUrls(pageNumber, pageSize, out total);
    }

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

    /// <summary>
    ///     Searches for all redirect URLs that contain a given search term in their URL property.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <param name="total">The total count of redirect URLs.</param>
    /// <returns>The redirect URLs.</returns>
    IEnumerable<IRedirectUrl> SearchRedirectUrls(string searchTerm, int skip, int take, out long total)
    {
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        return SearchRedirectUrls(searchTerm, pageNumber, pageSize, out total);
    }
}
