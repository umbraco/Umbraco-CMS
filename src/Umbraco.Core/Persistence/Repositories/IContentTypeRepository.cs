using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IContentType" /> entities (document types).
/// </summary>
public interface IContentTypeRepository : IContentTypeRepositoryBase<IContentType>
{
    /// <summary>
    ///     Gets all entities of the specified <see cref="IPropertyType" /> query
    /// </summary>
    /// <param name="query"></param>
    /// <returns>An enumerable list of <see cref="IContentType" /> objects</returns>
    IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query);

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
    ///     Gets content type identifiers by their aliases.
    /// </summary>
    /// <param name="aliases">The aliases of the content types.</param>
    /// <returns>A collection of content type identifiers.</returns>
    IEnumerable<int> GetAllContentTypeIds(string[] aliases);
}
