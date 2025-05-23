using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Performs entity search directly against the database.
/// </summary>
public interface IEntitySearchService
{
    PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100);
}
