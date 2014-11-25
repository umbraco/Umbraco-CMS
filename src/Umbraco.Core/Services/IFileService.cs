using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the File Service, which is an easy access to operations involving <see cref="IFile"/> objects like Scripts, Stylesheets and Templates
    /// </summary>
    public interface IFileService : IService
    {
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
        /// Gets a <see cref="ITemplate"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias of the template</param>
        /// <returns>A <see cref="ITemplate"/> object</returns>
        ITemplate GetTemplate(string alias);

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias
        /// </summary>
        /// <param name="id">Id of the template</param>
        /// <returns>A <see cref="ITemplate"/> object</returns>
        ITemplate GetTemplate(int id);

        /// <summary>
        /// Returns a template as a template node which can be traversed (parent, children)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        TemplateNode GetTemplateNode(string alias);

        /// <summary>
        /// Given a template node in a tree, this will find the template node with the given alias if it is found in the hierarchy, otherwise null
        /// </summary>
        /// <param name="anyNode"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        TemplateNode FindTemplateInTree(TemplateNode anyNode, string alias);

        /// <summary>
        /// Saves a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to save</param>
        /// <param name="userId">Optional id of the user saving the template</param>
        void SaveTemplate(ITemplate template, int userId = 0);

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
    }
}