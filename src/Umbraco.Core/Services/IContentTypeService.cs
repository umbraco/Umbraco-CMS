using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages <see cref="IContentType" /> objects.
/// </summary>
public interface IContentTypeService : IContentTypeBaseService<IContentType>
{
    /// <summary>
    ///     Gets all property type aliases.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetAllPropertyTypeAliases();

    /// <summary>
    ///     Gets all content type aliases
    /// </summary>
    /// <param name="objectTypes">
    ///     If this list is empty, it will return all content type aliases for media, members and content, otherwise
    ///     it will only return content type aliases for the object types specified
    /// </param>
    /// <returns></returns>
    IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes);

    /// <summary>
    ///     Returns all content type Ids for the aliases given
    /// </summary>
    /// <param name="aliases"></param>
    /// <returns></returns>
    IEnumerable<int> GetAllContentTypeIds(string[] aliases);
}
