using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages <see cref="IMemberType" /> objects.
/// </summary>
public interface IMemberTypeService : IContentTypeBaseService<IMemberType>
{
    string GetDefault();
}
