using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///    Service to get information about content types.
/// </summary>
public interface IContentTypeInfoService
{
    /// <summary>
    /// Gets all the available content types.
    /// </summary>
    /// <returns>The collection of content type infos.</returns>
    public ICollection<ContentTypeInfo> GetContentTypes();

    /// <summary>
    /// Gets all the available media types.
    /// </summary>
    /// <returns>The collection of media type infos.</returns>
    public ICollection<ContentTypeInfo> GetMediaTypes();
}
