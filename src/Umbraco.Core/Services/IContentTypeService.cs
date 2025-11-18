using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services.OperationStatus;

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

    Task<IEnumerable<IContentType>> GetByQueryAsync(IQuery<IContentType> query, CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<IContentType>());

    /// <summary>
    /// Creates a template for the given content type.
    /// </summary>
    /// <param name="contentTypeKey">The content type key.</param>
    /// <param name="templateName">The name of the template to create.</param>
    /// <param name="templateAlias">The alias of the template to create.</param>
    /// <param name="isDefaultTemplate">Whether to set the template as the default template for the content type.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt containing the template's key if successful, or an error status if not.</returns>
    Task<Attempt<Guid?, ContentTypeOperationStatus>> CreateTemplateAsync(
        Guid contentTypeKey,
        string templateName,
        string templateAlias,
        bool isDefaultTemplate,
        Guid userKey) => throw new NotImplementedException();
}
