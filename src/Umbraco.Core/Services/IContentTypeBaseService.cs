using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides a common base interface for <see cref="IContentTypeBase" />.
/// </summary>
public interface IContentTypeBaseService
{
    /// <summary>
    ///     Gets a content type.
    /// </summary>
    IContentTypeComposition? Get(int id);
}
