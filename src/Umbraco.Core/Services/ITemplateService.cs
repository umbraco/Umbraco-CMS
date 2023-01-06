using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ITemplateService : IService
{
    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    IEnumerable<ITemplate> GetTemplates(params string[] aliases);

    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    IEnumerable<ITemplate> GetTemplates(int masterTemplateId);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the alias, or null.</returns>
    ITemplate? GetTemplate(string? alias);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    ITemplate? GetTemplate(int id);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its guid identifier.
    /// </summary>
    /// <param name="id">The guid identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    ITemplate? GetTemplate(Guid id);

    /// <summary>
    ///     Gets the template descendants
    /// </summary>
    /// <param name="masterTemplateId"></param>
    /// <returns></returns>
    IEnumerable<ITemplate> GetTemplateDescendants(int masterTemplateId);

    /// <summary>
    ///     Saves a <see cref="ITemplate" />
    /// </summary>
    /// <param name="template"><see cref="ITemplate" /> to save</param>
    /// <param name="userId">Optional id of the user saving the template</param>
    void SaveTemplate(ITemplate template, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Sets the master template of a <see cref="ITemplate" />
    /// </summary>
    /// <param name="template"><see cref="ITemplate" /> to set master template for.</param>
    /// <param name="userId">The alias of the master template, or null if the template should not have a master template.</param>
    void SetMasterTemplate(ITemplate template, string? masterTemplateAlias);

    /// <summary>
    ///     Creates a template for a content type
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <param name="contentTypeName"></param>
    /// <param name="userId"></param>
    /// <returns>
    ///     The template created
    /// </returns>
    Attempt<OperationResult<OperationResultType, ITemplate>?> CreateTemplateForContentType(
        string contentTypeAlias,
        string? contentTypeName,
        int userId = Constants.Security.SuperUserId);

    ITemplate CreateTemplateWithIdentity(string? name, string? alias, string? content, ITemplate? masterTemplate = null, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a template by its alias
    /// </summary>
    /// <param name="alias">Alias of the <see cref="ITemplate" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the template</param>
    void DeleteTemplate(string alias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a collection of <see cref="Template" /> objects
    /// </summary>
    /// <param name="templates">List of <see cref="Template" /> to save</param>
    /// <param name="userId">Optional id of the user</param>
    void SaveTemplate(IEnumerable<ITemplate> templates, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets the content of a template as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <returns>The content of the template.</returns>
    Stream GetTemplateFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a template.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <param name="content">The content of the template.</param>
    void SetTemplateFileContent(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a template.
    /// </summary>
    /// <param name="filepath">The filesystem path to the template.</param>
    /// <returns>The size of the template.</returns>
    long GetTemplateFileSize(string filepath);
}
