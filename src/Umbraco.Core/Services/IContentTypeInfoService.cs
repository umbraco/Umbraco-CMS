using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IContentTypeInfoService
{
    /// <summary>
    /// Gets all the available content types.
    /// </summary>
    public ICollection<ContentTypeInfo> GetContentTypes();
}
