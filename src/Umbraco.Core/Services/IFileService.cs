using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the File Service, which is an easy access to operations involving <see cref="IFile"/> objects like Scripts, Stylesheets and Templates
    /// </summary>
    public interface IFileService : IService
    {
        IEnumerable<string> GetPartialViewSnippetNames(params string[] filterNames);
        void CreatePartialViewFolder(string folderPath);
        void CreatePartialViewMacroFolder(string folderPath);
        void DeletePartialViewFolder(string folderPath);
        void DeletePartialViewMacroFolder(string folderPath);
        IPartialView GetPartialView(string path);
        IPartialView GetPartialViewMacro(string path);
        IEnumerable<IPartialView> GetPartialViewMacros(params string[] names);
        Attempt<IPartialView> CreatePartialView(IPartialView partialView, string snippetName = null, int userId = 0);
        Attempt<IPartialView> CreatePartialViewMacro(IPartialView partialView, string snippetName = null, int userId = 0);
        bool DeletePartialView(string path, int userId = 0);
        bool DeletePartialViewMacro(string path, int userId = 0);
        Attempt<IPartialView> SavePartialView(IPartialView partialView, int userId = 0);
        Attempt<IPartialView> SavePartialViewMacro(IPartialView partialView, int userId = 0);
        bool ValidatePartialView(PartialView partialView);
        bool ValidatePartialViewMacro(PartialView partialView);

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        IEnumerable<Stylesheet> GetStylesheets(params string[] names);

        /// <summary>
        /// Gets a <see cref="Stylesheet"/> object by its name
        /// </summary>
        /// <param name="name">Name of the stylesheet incl. extension</param>
        /// <returns>A <see cref="Stylesheet"/> object</returns>
        Stylesheet GetStylesheetByName(string name);

        /// <summary>
        /// Saves a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to save</param>
        /// <param name="userId">Optional id of the user saving the stylesheet</param>
        void SaveStylesheet(Stylesheet stylesheet, int userId = 0);

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Stylesheet to delete</param>
        /// <param name="userId">Optional id of the user deleting the stylesheet</param>
        void DeleteStylesheet(string path, int userId = 0);

        /// <summary>
        /// Validates a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to validate</param>
        /// <returns>True if Stylesheet is valid, otherwise false</returns>
        bool ValidateStylesheet(Stylesheet stylesheet);

        /// <summary>
        /// Gets a list of all <see cref="Script"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Script"/> objects</returns>
        IEnumerable<Script> GetScripts(params string[] names);

        /// <summary>
        /// Gets a <see cref="Script"/> object by its name
        /// </summary>
        /// <param name="name">Name of the script incl. extension</param>
        /// <returns>A <see cref="Script"/> object</returns>
        Script GetScriptByName(string name);

        /// <summary>
        /// Saves a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to save</param>
        /// <param name="userId">Optional id of the user saving the script</param>
        void SaveScript(Script script, int userId = 0);

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Script to delete</param>
        /// <param name="userId">Optional id of the user deleting the script</param>
        void DeleteScript(string path, int userId = 0);

        /// <summary>
        /// Validates a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        bool ValidateScript(Script script);

        /// <summary>
        /// Creates a folder for scripts
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        void CreateScriptFolder(string folderPath);

        /// <summary>
        /// Deletes a folder for scripts
        /// </summary>
        /// <param name="folderPath"></param>
        void DeleteScriptFolder(string folderPath);

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        IEnumerable<ITemplate> GetTemplates(params string[] aliases);

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        IEnumerable<ITemplate> GetTemplates(int masterTemplateId);

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias.
        /// </summary>
        /// <param name="alias">The alias of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the alias, or null.</returns>
        ITemplate GetTemplate(string alias);

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its identifier.
        /// </summary>
        /// <param name="id">The identifer of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        ITemplate GetTemplate(int id);

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its guid identifier.
        /// </summary>
        /// <param name="id">The guid identifier of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        ITemplate GetTemplate(Guid id);

        /// <summary>
        /// Gets the template descendants
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        IEnumerable<ITemplate> GetTemplateDescendants(string alias);

        /// <summary>
        /// Gets the template descendants
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        IEnumerable<ITemplate> GetTemplateDescendants(int masterTemplateId);

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        IEnumerable<ITemplate> GetTemplateChildren(string alias);

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        IEnumerable<ITemplate> GetTemplateChildren(int masterTemplateId);

        /// <summary>
        /// Saves a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to save</param>
        /// <param name="userId">Optional id of the user saving the template</param>
        void SaveTemplate(ITemplate template, int userId = 0);

        /// <summary>
        /// Creates a template for a content type
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="contentTypeName"></param>
        /// <param name="userId"></param>
        /// <returns>
        /// The template created
        /// </returns>
        Attempt<OperationResult<OperationResultType, ITemplate>> CreateTemplateForContentType(string contentTypeAlias, string contentTypeName, int userId = 0);

        ITemplate CreateTemplateWithIdentity(string name, string content, ITemplate masterTemplate = null, int userId = 0);

        /// <summary>
        /// Deletes a template by its alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="ITemplate"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the template</param>
        void DeleteTemplate(string alias, int userId = 0);

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        bool ValidateTemplate(ITemplate template);

        /// <summary>
        /// Saves a collection of <see cref="Template"/> objects
        /// </summary>
        /// <param name="templates">List of <see cref="Template"/> to save</param>
        /// <param name="userId">Optional id of the user</param>
        void SaveTemplate(IEnumerable<ITemplate> templates, int userId = 0);

        /// <summary>
        /// Gets the content of a template as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <returns>The content of the template.</returns>
        Stream GetTemplateFileContentStream(string filepath);

        /// <summary>
        /// Sets the content of a template.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <param name="content">The content of the template.</param>
        void SetTemplateFileContent(string filepath, Stream content);

        /// <summary>
        /// Gets the size of a template.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <returns>The size of the template.</returns>
        long GetTemplateFileSize(string filepath);

        /// <summary>
        /// Gets the content of a stylesheet as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the stylesheet.</param>
        /// <returns>The content of the stylesheet.</returns>
        Stream GetStylesheetFileContentStream(string filepath);

        /// <summary>
        /// Sets the content of a stylesheet.
        /// </summary>
        /// <param name="filepath">The filesystem path to the stylesheet.</param>
        /// <param name="content">The content of the stylesheet.</param>
        void SetStylesheetFileContent(string filepath, Stream content);

        /// <summary>
        /// Gets the size of a stylesheet.
        /// </summary>
        /// <param name="filepath">The filesystem path to the stylesheet.</param>
        /// <returns>The size of the stylesheet.</returns>
        long GetStylesheetFileSize(string filepath);

        /// <summary>
        /// Gets the content of a script file as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the script.</param>
        /// <returns>The content of the script file.</returns>
        Stream GetScriptFileContentStream(string filepath);

        /// <summary>
        /// Sets the content of a script file.
        /// </summary>
        /// <param name="filepath">The filesystem path to the script.</param>
        /// <param name="content">The content of the script file.</param>
        void SetScriptFileContent(string filepath, Stream content);

        /// <summary>
        /// Gets the size of a script file.
        /// </summary>
        /// <param name="filepath">The filesystem path to the script file.</param>
        /// <returns>The size of the script file.</returns>
        long GetScriptFileSize(string filepath);

        /// <summary>
        /// Gets the content of a macro partial view as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the macro partial view.</param>
        /// <returns>The content of the macro partial view.</returns>
        Stream GetPartialViewMacroFileContentStream(string filepath);

        /// <summary>
        /// Sets the content of a macro partial view.
        /// </summary>
        /// <param name="filepath">The filesystem path to the macro partial view.</param>
        /// <param name="content">The content of the macro partial view.</param>
        void SetPartialViewMacroFileContent(string filepath, Stream content);

        /// <summary>
        /// Gets the size of a macro partial view.
        /// </summary>
        /// <param name="filepath">The filesystem path to the macro partial view.</param>
        /// <returns>The size of the macro partial view.</returns>
        long GetPartialViewMacroFileSize(string filepath);

        /// <summary>
        /// Gets the content of a partial view as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the partial view.</param>
        /// <returns>The content of the partial view.</returns>
        Stream GetPartialViewFileContentStream(string filepath);

        /// <summary>
        /// Sets the content of a partial view.
        /// </summary>
        /// <param name="filepath">The filesystem path to the partial view.</param>
        /// <param name="content">The content of the partial view.</param>
        void SetPartialViewFileContent(string filepath, Stream content);

        /// <summary>
        /// Gets the size of a partial view.
        /// </summary>
        /// <param name="filepath">The filesystem path to the partial view.</param>
        /// <returns>The size of the partial view.</returns>
        long GetPartialViewFileSize(string filepath);

        /// <summary>
        /// Gets the content of a macro partial view snippet as a string
        /// </summary>
        /// <param name="snippetName">The name of the snippet</param>
        /// <returns></returns>
        string GetPartialViewMacroSnippetContent(string snippetName);

        /// <summary>
        /// Gets the content of a partial view snippet as a string.
        /// </summary>
        /// <param name="snippetName">The name of the snippet</param>
        /// <returns>The content of the partial view.</returns>
        string GetPartialViewSnippetContent(string snippetName);
    }
}
