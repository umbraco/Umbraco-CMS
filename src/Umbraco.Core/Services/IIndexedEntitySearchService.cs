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
    [Obsolete("Please use the method that accepts all parameters. Will be removed in V17.")]
    PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100, bool ignoreUserStartNodes = false);

    // default implementation to avoid breaking changes falls back to old behaviour
    [Obsolete("Please use the method that accepts all parameters. Will be removed in V17.")]
    PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, Guid? parentId, int skip = 0, int take = 100, bool ignoreUserStartNodes = false)
        => Search(objectType,query, skip, take, ignoreUserStartNodes);

    [Obsolete("Please use the method that accepts all parameters. Will be removed in V17.")]
    PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, Guid? parentId, IEnumerable<Guid>? contentTypeIds, int skip = 0, int take = 100, bool ignoreUserStartNodes = false)
        => Search(objectType,query, skip, take, ignoreUserStartNodes);

    // default implementation to avoid breaking changes falls back to old behaviour
    [Obsolete("Please use async version of this method. Will be removed in V17.")]
    PagedModel<IEntitySlim> Search(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        bool? trashed,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false)
        => Search(objectType,query, skip, take, ignoreUserStartNodes);

    Task<PagedModel<IEntitySlim>> SearchAsync(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        bool? trashed,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false);
}
