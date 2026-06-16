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
    [Obsolete("Use RegisterWithStatusAsync to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
    void Register(string url, Guid contentKey, string? culture = null);

    /// <summary>
    ///     Registers a redirect URL.
    /// </summary>
    /// <param name="oldUrl">The previous Umbraco URL route the redirect is being created from.</param>
    /// <param name="contentKey">The content unique key.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}" /> containing the registered redirect URL on success, or
    ///     <see cref="RedirectUrlOperationStatus.CancelledByNotification" /> if a notification handler
    ///     canceled the operation.
    /// </returns>
    // TODO (V20): Remove the default implementation, and rename this back to "Register" when the obsolete Register overload is removed.
    async Task<Attempt<IRedirectUrl?, RedirectUrlOperationStatus>> RegisterWithStatusAsync(string oldUrl, Guid contentKey, string? culture = null)
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
    [Obsolete("Use DeleteContentRedirectUrlsWithStatusAsync to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
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
    async Task<RedirectUrlOperationStatus> DeleteContentRedirectUrlsWithStatusAsync(Guid contentKey)
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
    [Obsolete("Use DeleteWithStatusAsync(IRedirectUrl) to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
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
    async Task<RedirectUrlOperationStatus> DeleteWithStatusAsync(IRedirectUrl redirectUrl)
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
    [Obsolete("Use DeleteWithStatusAsync(Guid) to support cancellation via notifications. Scheduled for removal in Umbraco 20.")]
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
    async Task<RedirectUrlOperationStatus> DeleteWithStatusAsync(Guid id)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Delete(id);
#pragma warning restore CS0618 // Type or member is obsolete
        return RedirectUrlOperationStatus.Success;
    }

    /// <summary>
    ///     Deletes all redirect URLs.
    /// </summary>
    Task DeleteAllAsync();

    /// <summary>
    ///     Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url);

    /// <summary>
    ///     Gets the most recent redirect URL corresponding to an Umbraco redirect URL route.
    /// </summary>
    /// <param name="url">The Umbraco redirect URL route.</param>
    /// <param name="culture">The culture of the request.</param>
    /// <returns>The most recent redirect URL corresponding to the route.</returns>
    Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url, string? culture);

    /// <summary>
    ///     Gets all redirect URLs for a content item.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <returns>All redirect URLs for the content item.</returns>
    Task<IEnumerable<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey);

    /// <summary>
    ///     Gets a paged list of redirect URLs for a content item.
    /// </summary>
    /// <param name="contentKey">The content unique key.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>A paged model of redirect URLs for the content item.</returns>
    Task<PagedModel<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey, int skip, int take);

    /// <summary>
    ///     Gets all redirect URLs.
    /// </summary>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>A paged model of redirect URLs.</returns>
    Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int skip, int take);

    /// <summary>
    ///     Gets all redirect URLs below a given content item.
    /// </summary>
    /// <param name="rootContentId">The content unique identifier.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>A paged model of redirect URLs.</returns>
    Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int rootContentId, int skip, int take);

    /// <summary>
    ///     Searches for all redirect URLs that contain a given search term in their URL property.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>A paged model of redirect URLs.</returns>
    Task<PagedModel<IRedirectUrl>> SearchRedirectUrlsAsync(string searchTerm, int skip, int take);
}
