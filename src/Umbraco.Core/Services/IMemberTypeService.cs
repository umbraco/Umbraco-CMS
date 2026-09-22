using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages <see cref="IMemberType" /> objects.
/// </summary>
public interface IMemberTypeService : IAsyncContentTypeBaseService<IMemberType>
{
    /// <summary>
    ///     Gets the alias of the default <see cref="IMemberType" />.
    /// </summary>
    /// <returns>The alias of the default member type.</returns>
    string GetDefault();
}
