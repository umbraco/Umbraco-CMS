using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

// TODO
// this should be IContentTypeRepository, and what is IContentTypeRepository at the moment should
// become IDocumentTypeRepository - but since these interfaces are public, that would be breaking

/// <summary>
///     Represents the content types common repository, dealing with document, media and member types.
/// </summary>
public interface IContentTypeCommonRepository
{
    /// <summary>
    ///     Gets and cache all types.
    /// </summary>
    IEnumerable<IContentTypeComposition>? GetAllTypes();

    /// <summary>
    ///     Clears the cache.
    /// </summary>
    void ClearCache();
}
