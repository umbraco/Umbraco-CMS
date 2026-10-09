using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Search.Indexing;

/// <summary>
/// Resolves public access restrictions for a content item into indexable <see cref="ContentProtection"/> metadata.
/// </summary>
public interface IContentProtectionProvider
{
    /// <summary>
    /// Resolves the member and member group keys allowed to access the given content item, if it is publicly restricted.
    /// </summary>
    /// <param name="content">The content item to resolve protection for.</param>
    /// <returns>The content protection metadata, or null if the content is not publicly restricted.</returns>
    Task<ContentProtection?> GetContentProtectionAsync(IContentBase content);
}
