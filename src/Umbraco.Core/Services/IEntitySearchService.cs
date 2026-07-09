using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Performs entity search directly against the database.
/// </summary>
public interface IEntitySearchService
{
    /// <summary>
    /// Searches for entities of a given object type using a query string.
    /// </summary>
    /// <param name="objectType">The type of entities to search for.</param>
    /// <param name="query">The search query string.</param>
    /// <param name="skip">The number of results to skip for pagination.</param>
    /// <param name="take">The maximum number of results to return.</param>
    /// <returns>A paged model containing the matching entities.</returns>
    PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100);

    /// <summary>
    /// Searches entities of multiple object types by query string.
    /// </summary>
    /// <remarks>
    /// This method has a no-op default implementation.
    /// </remarks>
    // TODO (V18): Remove the default implementation.
    PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectTypes, string query, int skip = 0, int take = 100)
        => new() { Items = [], Total = 0 };

    /// <summary>
    /// Gets all entities of multiple object types with pagination.
    /// </summary>
    /// <remarks>
    /// This method has a no-op default implementation.
    /// </remarks>
    // TODO (V18): Remove the default implementation.
    PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectTypes, int skip = 0, int take = 100)
        => new() { Items = [], Total = 0 };
}
