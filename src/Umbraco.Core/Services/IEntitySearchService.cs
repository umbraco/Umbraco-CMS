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

    PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectType, string query, int skip = 0, int take = 100);

    PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectType, int skip = 0, int take = 100);
}
