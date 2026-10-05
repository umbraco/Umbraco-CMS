using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides methods for managing templates (views) used for rendering content.
/// </summary>
public interface ITemplateService : IService
{
    /// <summary>
    ///     Gets a template by its key.
    /// </summary>
    /// <param name="key">The key of the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The template, or <c>null</c> if not found.</returns>
    Task<ITemplate?> GetAsync(Guid key, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a template by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The template, or <c>null</c> if not found.</returns>
    Task<ITemplate?> GetAsync(string alias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all templates.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All templates.</returns>
    Task<IEnumerable<ITemplate>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the templates with the specified keys.
    /// </summary>
    /// <param name="keys">The keys of the templates.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The templates found; keys that do not match a template are ignored.</returns>
    Task<IEnumerable<ITemplate>> GetManyAsync(IEnumerable<Guid> keys, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a template.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="alias">The alias of the template.</param>
    /// <param name="content">The view content of the template.</param>
    /// <param name="templateKey">The key of the template, or <c>null</c> to generate one.</param>
    /// <param name="userKey">The key of the user creating the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt holding the created template and the operation status.</returns>
    Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(
        string name,
        string alias,
        string? content,
        Guid? templateKey,
        Guid userKey,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a template.
    /// </summary>
    /// <param name="template">The template to create.</param>
    /// <param name="userKey">The key of the user creating the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt holding the created template and the operation status.</returns>
    Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(ITemplate template, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a template for a content type.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="alias">The alias of the template.</param>
    /// <param name="contentTypeAlias">The alias of the content type the template is created for.</param>
    /// <param name="userKey">The key of the user creating the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt holding the created template and the operation status.</returns>
    Task<Attempt<ITemplate?, TemplateOperationStatus>> CreateForContentTypeAsync(
        string name,
        string alias,
        string contentTypeAlias,
        Guid userKey,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Updates a template.
    /// </summary>
    /// <param name="template">The template to update.</param>
    /// <param name="userKey">The key of the user updating the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt holding the updated template and the operation status.</returns>
    Task<Attempt<ITemplate, TemplateOperationStatus>> UpdateAsync(ITemplate template, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a template.
    /// </summary>
    /// <param name="key">The key of the template to delete.</param>
    /// <param name="userKey">The key of the user deleting the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt holding the deleted template and the operation status.</returns>
    Task<Attempt<ITemplate?, TemplateOperationStatus>> DeleteAsync(Guid key, Guid userKey, CancellationToken cancellationToken);
}
