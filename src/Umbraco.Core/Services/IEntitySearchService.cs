using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Performs entity search directly against the database.
/// </summary>
public interface IEntitySearchService
{
    PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100);

    /// <summary>
    /// Searches entities of multiple object types by query string.
    /// </summary>
    /// <remarks>
    /// This method has a no-op default implementation.
    /// </remarks>
    PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectTypes, string query, int skip = 0, int take = 100)
        => new() { Items = [], Total = 0 };

    /// <summary>
    /// Gets all entities of multiple object types with pagination.
    /// </summary>
    /// <remarks>
    /// This method has a no-op default implementation.
    /// </remarks>
    PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectTypes, int skip = 0, int take = 100)
        => new() { Items = [], Total = 0 };
}
