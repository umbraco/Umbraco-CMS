using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Performs entity search against search indexes.
/// </summary>
/// <remarks>
/// Note that this service only supports entity types that are included in search indexes.
/// By default this means documents, media and members.
/// </remarks>
public interface IIndexedEntitySearchService
{
    PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100, bool ignoreUserStartNodes = false);
}
