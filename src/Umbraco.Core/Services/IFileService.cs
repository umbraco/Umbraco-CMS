using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the File Service, which is an easy access to operations involving <see cref="IFile" /> objects like
///     Scripts, Stylesheets and Templates
/// </summary>
public interface IFileService : IService
{
    /// <summary>
    ///     Gets a <see cref="IPartialView" /> object by its path.
    /// </summary>
    /// <param name="path">The path of the partial view.</param>
    /// <returns>The <see cref="IPartialView" /> object matching the path, or null.</returns>
    [Obsolete("Please use IPartialViewService for partial view operations. Scheduled for removal in Umbraco 18.")]
    IPartialView? GetPartialView(string path);

    /// <summary>
    ///     Saves a <see cref="IPartialView" />.
    /// </summary>
    /// <param name="partialView">The <see cref="IPartialView" /> to save.</param>
    /// <param name="userId">Optional id of the user saving the partial view.</param>
    /// <returns>An <see cref="Attempt{T}" /> indicating success or failure with the saved partial view.</returns>
    [Obsolete("Please use IPartialViewService for partial view operations. Scheduled for removal in Umbraco 18.")]
    Attempt<IPartialView?> SavePartialView(IPartialView partialView, int? userId = null);

    /// <summary>
    ///     Gets a <see cref="IStylesheet" /> object by its name
    /// </summary>
    /// <param name="path">Path of the stylesheet incl. extension</param>
    /// <returns>A <see cref="IStylesheet" /> object</returns>
    [Obsolete("Please use IStylesheetService for stylesheet operations. Scheduled for removal in Umbraco 18.")]
    IStylesheet? GetStylesheet(string? path);

    /// <summary>
    ///     Saves a <see cref="IStylesheet" />
    /// </summary>
    /// <param name="stylesheet"><see cref="IStylesheet" /> to save</param>
    /// <param name="userId">Optional id of the user saving the stylesheet</param>
    [Obsolete("Please use IStylesheetService for stylesheet operations. Scheduled for removal in Umbraco 18.")]
    void SaveStylesheet(IStylesheet? stylesheet, int? userId = null);

    /// <summary>
    ///     Gets a <see cref="IScript" /> object by its name
    /// </summary>
    /// <param name="name">Name of the script incl. extension</param>
    /// <returns>A <see cref="IScript" /> object</returns>
    [Obsolete("Please use IScriptService for script operations. Scheduled for removal in Umbraco 18.")]
    IScript? GetScript(string? name);

    /// <summary>
    ///     Saves a <see cref="Script" />
    /// </summary>
    /// <param name="script"><see cref="IScript" /> to save</param>
    /// <param name="userId">Optional id of the user saving the script</param>
    [Obsolete("Please use IScriptService for script operations. Scheduled for removal in Umbraco 18.")]
    void SaveScript(IScript? script, int? userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the alias, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    ITemplate? GetTemplate(string? alias);

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    ITemplate? GetTemplate(int id);
}
