using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the File Service, which is an easy access to operations involving <see cref="IFile" /> objects like
///     Scripts, Stylesheets and Templates
/// </summary>
public interface IFileService : IService
{
    [Obsolete("Please use SnippetCollection.GetPartialViewSnippetNames() or SnippetCollection.GetPartialViewMacroSnippetNames() instead. Scheduled for removal in V12.")]
    IEnumerable<string> GetPartialViewSnippetNames(params string[] filterNames);

    void CreatePartialViewFolder(string folderPath);

    void CreatePartialViewMacroFolder(string folderPath);

    void DeletePartialViewFolder(string folderPath);

    void DeletePartialViewMacroFolder(string folderPath);

    /// <summary>
    ///     Gets a list of all <see cref="IPartialView" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="IPartialView" /> objects</returns>
    IEnumerable<IPartialView> GetPartialViews(params string[] names);

    IPartialView? GetPartialView(string path);

    IPartialView? GetPartialViewMacro(string path);

    Attempt<IPartialView?> CreatePartialView(IPartialView partialView, string? snippetName = null, int? userId = Constants.Security.SuperUserId);

    Attempt<IPartialView?> CreatePartialViewMacro(IPartialView partialView, string? snippetName = null, int? userId = Constants.Security.SuperUserId);

    bool DeletePartialView(string path, int? userId = null);

    bool DeletePartialViewMacro(string path, int? userId = null);

    Attempt<IPartialView?> SavePartialView(IPartialView partialView, int? userId = null);

    Attempt<IPartialView?> SavePartialViewMacro(IPartialView partialView, int? userId = null);

    /// <summary>
    ///     Gets the content of a partial view as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the partial view.</param>
    /// <returns>The content of the partial view.</returns>
    Stream GetPartialViewFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a partial view.
    /// </summary>
    /// <param name="filepath">The filesystem path to the partial view.</param>
    /// <param name="content">The content of the partial view.</param>
    void SetPartialViewFileContent(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a partial view.
    /// </summary>
    /// <param name="filepath">The filesystem path to the partial view.</param>
    /// <returns>The size of the partial view.</returns>
    long GetPartialViewFileSize(string filepath);

    /// <summary>
    ///     Gets the content of a macro partial view as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the macro partial view.</param>
    /// <returns>The content of the macro partial view.</returns>
    Stream GetPartialViewMacroFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a macro partial view.
    /// </summary>
    /// <param name="filepath">The filesystem path to the macro partial view.</param>
    /// <param name="content">The content of the macro partial view.</param>
    void SetPartialViewMacroFileContent(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a macro partial view.
    /// </summary>
    /// <param name="filepath">The filesystem path to the macro partial view.</param>
    /// <returns>The size of the macro partial view.</returns>
    long GetPartialViewMacroFileSize(string filepath);

    /// <summary>
    ///     Gets a list of all <see cref="IStylesheet" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="IStylesheet" /> objects</returns>
    IEnumerable<IStylesheet> GetStylesheets(params string[] paths);

    /// <summary>
    ///     Gets a <see cref="IStylesheet" /> object by its name
    /// </summary>
    /// <param name="path">Path of the stylesheet incl. extension</param>
    /// <returns>A <see cref="IStylesheet" /> object</returns>
    IStylesheet? GetStylesheet(string? path);

    /// <summary>
    ///     Saves a <see cref="IStylesheet" />
    /// </summary>
    /// <param name="stylesheet"><see cref="IStylesheet" /> to save</param>
    /// <param name="userId">Optional id of the user saving the stylesheet</param>
    void SaveStylesheet(IStylesheet? stylesheet, int? userId = null);

    /// <summary>
    ///     Deletes a stylesheet by its name
    /// </summary>
    /// <param name="path">Name incl. extension of the Stylesheet to delete</param>
    /// <param name="userId">Optional id of the user deleting the stylesheet</param>
    void DeleteStylesheet(string path, int? userId = null);

    /// <summary>
    ///     Creates a folder for style sheets
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    void CreateStyleSheetFolder(string folderPath);

    /// <summary>
    ///     Deletes a folder for style sheets
    /// </summary>
    /// <param name="folderPath"></param>
    void DeleteStyleSheetFolder(string folderPath);

    /// <summary>
    ///     Gets the content of a stylesheet as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the stylesheet.</param>
    /// <returns>The content of the stylesheet.</returns>
    Stream GetStylesheetFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a stylesheet.
    /// </summary>
    /// <param name="filepath">The filesystem path to the stylesheet.</param>
    /// <param name="content">The content of the stylesheet.</param>
    void SetStylesheetFileContent(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a stylesheet.
    /// </summary>
    /// <param name="filepath">The filesystem path to the stylesheet.</param>
    /// <returns>The size of the stylesheet.</returns>
    long GetStylesheetFileSize(string filepath);

    /// <summary>
    ///     Gets a list of all <see cref="IScript" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="IScript" /> objects</returns>
    IEnumerable<IScript> GetScripts(params string[] names);

    /// <summary>
    ///     Gets a <see cref="IScript" /> object by its name
    /// </summary>
    /// <param name="name">Name of the script incl. extension</param>
    /// <returns>A <see cref="IScript" /> object</returns>
    IScript? GetScript(string? name);

    /// <summary>
    ///     Saves a <see cref="Script" />
    /// </summary>
    /// <param name="script"><see cref="IScript" /> to save</param>
    /// <param name="userId">Optional id of the user saving the script</param>
    void SaveScript(IScript? script, int? userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a script by its name
    /// </summary>
    /// <param name="path">Name incl. extension of the Script to delete</param>
    /// <param name="userId">Optional id of the user deleting the script</param>
    void DeleteScript(string path, int? userId = null);

    /// <summary>
    ///     Creates a folder for scripts
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    void CreateScriptFolder(string folderPath);

    /// <summary>
    ///     Deletes a folder for scripts
    /// </summary>
    /// <param name="folderPath"></param>
    void DeleteScriptFolder(string folderPath);

    /// <summary>
    ///     Gets the content of a script file as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the script.</param>
    /// <returns>The content of the script file.</returns>
    Stream GetScriptFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a script file.
    /// </summary>
    /// <param name="filepath">The filesystem path to the script.</param>
    /// <param name="content">The content of the script file.</param>
    void SetScriptFileContent(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a script file.
    /// </summary>
    /// <param name="filepath">The filesystem path to the script file.</param>
    /// <returns>The size of the script file.</returns>
    long GetScriptFileSize(string filepath);

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

    /// <summary>
    ///     Gets the content of a macro partial view snippet as a string
    /// </summary>
    /// <param name="snippetName">The name of the snippet</param>
    /// <returns></returns>
    [Obsolete("Please use SnippetCollection.GetPartialViewMacroSnippetContent instead. Scheduled for removal in V12.")]
    string GetPartialViewMacroSnippetContent(string snippetName);

    /// <summary>
    ///     Gets the content of a partial view snippet as a string.
    /// </summary>
    /// <param name="snippetName">The name of the snippet</param>
    /// <returns>The content of the partial view.</returns>
    [Obsolete("Please use SnippetCollection.GetPartialViewSnippetContent instead. Scheduled for removal in V12.")]
    string GetPartialViewSnippetContent(string snippetName);
}
