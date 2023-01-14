using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ITemplateService : IService
{
    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    Task<IEnumerable<ITemplate>> GetTemplatesAsync(params string[] aliases);

    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    Task<IEnumerable<ITemplate>> GetTemplatesAsync(int masterTemplateId);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the alias, or null.</returns>
    Task<ITemplate?> GetTemplateAsync(string? alias);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    Task<ITemplate?> GetTemplateAsync(int id);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its guid identifier.
    /// </summary>
    /// <param name="id">The guid identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    Task<ITemplate?> GetTemplateAsync(Guid id);

    /// <summary>
    ///     Gets the template descendants
    /// </summary>
    /// <param name="masterTemplateId"></param>
    /// <returns></returns>
    Task<IEnumerable<ITemplate>> GetTemplateDescendantsAsync(int masterTemplateId);

    /// <summary>
    ///     Saves a <see cref="ITemplate" />
    /// </summary>
    /// <param name="template"><see cref="ITemplate" /> to save</param>
    /// <param name="userId">Optional id of the user saving the template</param>
    /// <returns>True if the template was saved, false otherwise.</returns>
    Task<bool> SaveTemplateAsync(ITemplate template, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a template for a content type
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <param name="contentTypeName"></param>
    /// <param name="userId"></param>
    /// <returns>
    ///     The template created
    /// </returns>
    Task<Attempt<OperationResult<OperationResultType, ITemplate>?>> CreateTemplateForContentTypeAsync(
        string contentTypeAlias,
        string? contentTypeName,
        int userId = Constants.Security.SuperUserId);

    Task<ITemplate?> CreateTemplateWithIdentityAsync(string? name, string? alias, string? content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a template by its alias
    /// </summary>
    /// <param name="alias">Alias of the <see cref="ITemplate" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the template</param>
    /// <returns>True if the template was deleted, false otherwise</returns>
    Task<bool> DeleteTemplateAsync(string alias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a template by its key
    /// </summary>
    /// <param name="key">Key of the <see cref="ITemplate" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the template</param>
    /// <returns>True if the template was deleted, false otherwise</returns>
    Task<bool> DeleteTemplateAsync(Guid key, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a collection of <see cref="Template" /> objects
    /// </summary>
    /// <param name="templates">List of <see cref="Template" /> to save</param>
    /// <param name="userId">Optional id of the user</param>
    /// <returns>True if the templates were saved, false otherwise</returns>
    Task<bool> SaveTemplateAsync(IEnumerable<ITemplate> templates, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets the content of a template as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <returns>The content of the template.</returns>
    Task<Stream> GetTemplateFileContentStreamAsync(string filepath);

    /// <summary>
    ///     Sets the content of a template.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <param name="content">The content of the template.</param>
    Task SetTemplateFileContentAsync(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a template.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <returns>The size of the template.</returns>
    Task<long> GetTemplateFileSizeAsync(string filepath);
}
