using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface ITemplateService : IService
{
    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" />.</returns>
    Task<IEnumerable<ITemplate>> GetAllAsync(params string[] aliases);

    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" />.</returns>
    Task<IEnumerable<ITemplate>> GetAllAsync(Guid[] keys);

    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    Task<IEnumerable<ITemplate>> GetChildrenAsync(int masterTemplateId);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the alias, or null.</returns>
    Task<ITemplate?> GetAsync(string? alias);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    Task<ITemplate?> GetAsync(int id);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its guid identifier.
    /// </summary>
    /// <param name="id">The guid identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    Task<ITemplate?> GetAsync(Guid id);

    /// <summary>
    ///     Gets the template descendants
    /// </summary>
    /// <param name="masterTemplateId"></param>
    /// <returns></returns>
    Task<IEnumerable<ITemplate>> GetDescendantsAsync(int masterTemplateId);

    /// <summary>
    ///     Updates a <see cref="ITemplate" />
    /// </summary>
    /// <param name="template"><see cref="ITemplate" /> to update</param>
    /// <param name="userKey">Key of the user saving the template</param>
    /// <returns></returns>
    Task<Attempt<ITemplate, TemplateOperationStatus>> UpdateAsync(ITemplate template, Guid userKey);

    /// <summary>
    ///     Creates a template for a content type
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <param name="contentTypeName"></param>
    /// <param name="userKey">Key of the user performing the Create.</param>
    /// <returns>
    ///     The template created
    /// </returns>
    Task<Attempt<ITemplate, TemplateOperationStatus>> CreateForContentTypeAsync(
        string contentTypeAlias,
        string? contentTypeName,
        Guid userKey);

    /// <summary>
    ///     Creates a new template
    /// </summary>
    /// <param name="templateKey"></param>
    /// <param name="name">Name of the new template</param>
    /// <param name="alias">Alias of the template</param>
    /// <param name="content">View content for the new template</param>
    /// <param name="userKey">Key of the user performing the Create.</param>
    /// <returns></returns>
    Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(string name, string alias, string? content, Guid userKey, Guid? templateKey = null);

    /// <summary>
    ///     Creates a new template
    /// </summary>
    /// <param name="template">The new template</param>
    /// <param name="userKey">Key of the user performing the Create.</param>
    /// <returns></returns>
    Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(ITemplate template, Guid userKey);

    /// <summary>
    ///     Deletes a template by its alias
    /// </summary>
    /// <param name="alias">Alias of the <see cref="ITemplate" /> to delete</param>
    /// <param name="userKey">Key of the user performing the Delete.</param>
    /// <returns>True if the template was deleted, false otherwise</returns>
    Task<Attempt<ITemplate?, TemplateOperationStatus>> DeleteAsync(string alias, Guid userKey);

    /// <summary>
    ///     Deletes a template by its key
    /// </summary>
    /// <param name="key">Key of the <see cref="ITemplate" /> to delete</param>
    /// <param name="userKey">Key of the user performing the Delete.</param>
    /// <returns>True if the template was deleted, false otherwise</returns>
    Task<Attempt<ITemplate?, TemplateOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Gets the content of a template as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <returns>The content of the template.</returns>
    Task<Stream> GetFileContentStreamAsync(string filepath);

    /// <summary>
    ///     Sets the content of a template.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <param name="content">The content of the template.</param>
    Task SetFileContentAsync(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a template.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <returns>The size of the template.</returns>
    Task<long> GetFileSizeAsync(string filepath);
}
