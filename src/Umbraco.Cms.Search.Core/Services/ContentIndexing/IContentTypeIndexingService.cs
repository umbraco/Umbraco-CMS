using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Triggers re-indexing of all content of the given content/media/member types, e.g. after a content type structure change.
/// </summary>
public interface IContentTypeIndexingService
{
    /// <summary>
    /// Queues background re-indexing of every content item of the given types (and any types composing them).
    /// </summary>
    /// <param name="contentTypeKeys">The keys of the content/media/member types to re-index.</param>
    /// <param name="objectType">The object type the keys refer to (document, media or member).</param>
    /// <param name="origin">An identifier for the server/request that requested the re-index, used for same-origin filtering.</param>
    void ReindexByContentTypes(Guid[] contentTypeKeys, UmbracoObjectTypes objectType, string origin);
}
