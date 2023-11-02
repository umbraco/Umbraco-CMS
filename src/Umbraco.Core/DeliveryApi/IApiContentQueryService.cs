using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Service that handles querying of the Delivery API.
/// </summary>
public interface IApiContentQueryService
{
    [Obsolete($"Use the {nameof(ExecuteQuery)} method that accepts {nameof(ProtectedAccess)}. Will be removed in V14.")]
    Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take);

    /// <summary>
    ///     Returns an attempt with a collection of item ids that passed the search criteria as a paged model.
    /// </summary>
    /// <param name="fetch">Optional fetch query parameter value.</param>
    /// <param name="filters">Optional filter query parameters values.</param>
    /// <param name="sorts">Optional sort query parameters values.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="protectedAccess">Defines the limitations for querying protected content.</param>
    /// <returns>A paged model of item ids that are returned after applying the search queries in an attempt.</returns>
    Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, ProtectedAccess protectedAccess, int skip, int take)
        => default;
}
