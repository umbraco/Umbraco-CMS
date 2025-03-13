using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Service that handles querying of the Media APIs.
/// </summary>
public interface IApiMediaQueryService
{
    /// <summary>
    ///     Returns an attempt with a collection of media ids that passed the search criteria as a paged model.
    /// </summary>
    /// <param name="fetch">Optional fetch query parameter value.</param>
    /// <param name="filters">Optional filter query parameters values.</param>
    /// <param name="sorts">Optional sort query parameters values.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>A paged model of media ids that are returned after applying the search queries in an attempt.</returns>
    Attempt<PagedModel<Guid>, ApiMediaQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take);

    /// <summary>
    ///     Returns the media item that matches the supplied path (if any).
    /// </summary>
    /// <param name="path">The path to look up.</param>
    /// <returns>The media item at <see cref="path"/>, or null if it does not exist.</returns>
    IPublishedContent? GetByPath(string path);
}
