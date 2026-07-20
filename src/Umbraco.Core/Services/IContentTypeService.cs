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
    /// <returns>An enumerable collection of all property type aliases.</returns>
    IEnumerable<string> GetAllPropertyTypeAliases();

    /// <summary>
    ///     Gets all content type aliases
    /// </summary>
    /// <param name="objectTypes">
    ///     If this list is empty, it will return all content type aliases for media, members and content, otherwise
    ///     it will only return content type aliases for the object types specified
    /// </param>
    /// <returns>An enumerable collection of content type aliases.</returns>
    IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes);

    /// <summary>
    ///     Returns all content type Ids for the aliases given.
    /// </summary>
    /// <param name="aliases">The content type aliases to look up.</param>
    /// <returns>An enumerable collection of content type identifiers.</returns>
    IEnumerable<int> GetAllContentTypeIds(string[] aliases);

    /// <summary>
    ///     Gets content types matching the specified query.
    /// </summary>
    /// <param name="query">The query to filter content types.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching content types.</returns>
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
