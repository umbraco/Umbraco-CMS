using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the File Service, which is an easy access to operations involving <see cref="IFile"/> objects like Scripts, Stylesheets and Templates
    /// </summary>
    public class FileService : ScopeRepositoryService, IFileService
    {
        private const string PartialViewHeader = "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage";
        private const string PartialViewMacroHeader = "@inherits Umbraco.Web.Macros.PartialViewMacroPage";

        public FileService(
            IDatabaseUnitOfWorkProvider provider,
            RepositoryFactory repositoryFactory,
            ILogger logger,
            IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        { }


        #region Stylesheets

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        public IEnumerable<Stylesheet> GetStylesheets(params string[] names)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                return repository.GetAll(names);
            }
        }

        /// <summary>
        /// Gets a <see cref="Stylesheet"/> object by its name
        /// </summary>
        /// <param name="name">Name of the stylesheet incl. extension</param>
        /// <returns>A <see cref="Stylesheet"/> object</returns>
        public Stylesheet GetStylesheetByName(string name)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                return repository.Get(name);
            }
        }

        /// <summary>
        /// Saves a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to save</param>
        /// <param name="userId"></param>
        public void SaveStylesheet(Stylesheet stylesheet, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<Stylesheet>(stylesheet);
                if (uow.Events.DispatchCancelable(SavingStylesheet, this, saveEventArgs))
                {
                    uow.Commit();
                    return;
                }

                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                repository.AddOrUpdate(stylesheet);
                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(SavedStylesheet, this, saveEventArgs);

                Audit(uow, AuditType.Save, "Save Stylesheet performed by user", userId, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Stylesheet to delete</param>
        /// <param name="userId"></param>
        public void DeleteStylesheet(string path, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                var stylesheet = repository.Get(path);
                if (stylesheet == null)
                {
                    uow.Commit();
                    return;
                }

                var deleteEventArgs = new DeleteEventArgs<Stylesheet>(stylesheet);
                if (uow.Events.DispatchCancelable(DeletingStylesheet, this, deleteEventArgs))
                {
                    uow.Commit();
                    return;
                }

                repository.Delete(stylesheet);
                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedStylesheet, this, deleteEventArgs);

                Audit(uow, AuditType.Delete, string.Format("Delete Stylesheet performed by user"), userId, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// Validates a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to validate</param>
        /// <returns>True if Stylesheet is valid, otherwise false</returns>
        public bool ValidateStylesheet(Stylesheet stylesheet)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                return repository.ValidateStylesheet(stylesheet);
            }
        }

        #endregion

        #region Scripts
        /// <summary>
        /// Gets a list of all <see cref="Script"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Script"/> objects</returns>
        public IEnumerable<Script> GetScripts(params string[] names)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                return repository.GetAll(names);
            }
        }

        /// <summary>
        /// Gets a <see cref="Script"/> object by its name
        /// </summary>
        /// <param name="name">Name of the script incl. extension</param>
        /// <returns>A <see cref="Script"/> object</returns>
        public Script GetScriptByName(string name)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                return repository.Get(name);
            }
        }

        /// <summary>
        /// Saves a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to save</param>
        /// <param name="userId"></param>
        public void SaveScript(Script script, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<Script>(script);
                if (uow.Events.DispatchCancelable(SavingScript, this, saveEventArgs))
                {
                    uow.Commit();
                    return;
                }

                var repository = RepositoryFactory.CreateScriptRepository(uow);
                repository.AddOrUpdate(script);
                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(SavedScript, this, saveEventArgs);

                Audit(uow, AuditType.Save, "Save Script performed by user", userId, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Script to delete</param>
        /// <param name="userId"></param>
        public void DeleteScript(string path, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                var script = repository.Get(path);
                if (script == null)
                {
                    uow.Commit();
                    return;
                }

                var deleteEventArgs = new DeleteEventArgs<Script>(script);
                if (uow.Events.DispatchCancelable(DeletingScript, this, deleteEventArgs))
                {
                    uow.Commit();
                    return;
                }

                repository.Delete(script);
                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedScript, this, deleteEventArgs);

                Audit(uow, AuditType.Delete, string.Format("Delete Script performed by user"), userId, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// Validates a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateScript(Script script)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                return repository.ValidateScript(script);
            }
        }

        public void CreateScriptFolder(string folderPath)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                ((ScriptRepository)repository).AddFolder(folderPath);
                uow.Commit();
            }
        }

        public void DeleteScriptFolder(string folderPath)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                ((ScriptRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        #endregion

        #region Templates

        /// <summary>
        /// Creates a template for a content type
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="contentTypeName"></param>
        /// <param name="userId"></param>
        /// <returns>
        /// The template created
        /// </returns>
        public Attempt<OperationStatus<ITemplate, OperationStatusType>> CreateTemplateForContentType(string contentTypeAlias, string contentTypeName, int userId = 0)
        {
            var template = new Template(contentTypeName,
                //NOTE: We are NOT passing in the content type alias here, we want to use it's name since we don't
                // want to save template file names as camelCase, the Template ctor will clean the alias as
                // `alias.ToCleanString(CleanStringType.UnderscoreAlias)` which has been the default.
                // This fixes: http://issues.umbraco.org/issue/U4-7953
                contentTypeName);

            var evtMsgs = EventMessagesFactory.Get();

            //NOTE: This isn't pretty but we need to maintain backwards compatibility so we cannot change
            // the event args here. The other option is to create a different event with different event
            // args specifically for this method... which also isn't pretty. So for now, we'll use this
            // dictionary approach to store 'additional data' in.
            var additionalData = new Dictionary<string, object>
            {
                {"CreateTemplateForContentType", true},
                {"ContentTypeAlias", contentTypeAlias},
            };

            // check that the template hasn't been created on disk before creating the content type
            // if it exists, set the new template content to the existing file content
            string content = GetViewContent(contentTypeAlias);
            if (content != null)
            {
                template.Content = content;
            }

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<ITemplate>(template, true, evtMsgs, additionalData);
                if (uow.Events.DispatchCancelable(SavingTemplate, this, saveEventArgs))
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<ITemplate, OperationStatusType>(template, OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                repository.AddOrUpdate(template);
                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(SavedTemplate, this, saveEventArgs);

                Audit(uow, AuditType.Save, "Save Template performed by user", userId, template.Id);
                uow.Commit();
            }

            return Attempt.Succeed(new OperationStatus<ITemplate, OperationStatusType>(template, OperationStatusType.Success, evtMsgs));
        }

        /// <summary>
        /// Create a new template, setting the content if a view exists in the filesystem
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="masterTemplate"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ITemplate CreateTemplateWithIdentity(string name, string content, ITemplate masterTemplate = null, int userId = 0)
        {
            // file might already be on disk, if so grab the content to avoid overwriting
            var template = new Template(name, name)
            {
                Content = GetViewContent(name) ?? content
            };

            if (masterTemplate != null)
            {
                template.SetMasterTemplate(masterTemplate);
            }

            SaveTemplate(template, userId);

            return template;
        }

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(params string[] aliases)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetAll(aliases).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(int masterTemplateId)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetChildren(masterTemplateId).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias.
        /// </summary>
        /// <param name="alias">The alias of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the alias, or null.</returns>
        public ITemplate GetTemplate(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.Get(alias);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its identifier.
        /// </summary>
        /// <param name="id">The identifer of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        public ITemplate GetTemplate(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its guid identifier.
        /// </summary>
        /// <param name="id">The guid identifier of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        public ITemplate GetTemplate(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                var query = Query<ITemplate>.Builder.Where(x => x.Key == id);
                return repository.GetByQuery(query).SingleOrDefault();
            }
        }

        public IEnumerable<ITemplate> GetTemplateDescendants(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetDescendants(alias);
            }
        }

        /// <summary>
        /// Gets the template descendants
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateDescendants(int masterTemplateId)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetDescendants(masterTemplateId);
            }
        }

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateChildren(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetChildren(alias);
            }
        }

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateChildren(int masterTemplateId)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetChildren(masterTemplateId);
            }
        }

        /// <summary>
        /// Returns a template as a template node which can be traversed (parent, children)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TemplateNode GetTemplateNode(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetTemplateNode(alias);
            }
        }

        /// <summary>
        /// Given a template node in a tree, this will find the template node with the given alias if it is found in the hierarchy, otherwise null
        /// </summary>
        /// <param name="anyNode"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TemplateNode FindTemplateInTree(TemplateNode anyNode, string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.FindTemplateInTree(anyNode, alias);
            }
        }

        /// <summary>
        /// Saves a <see cref="Template"/>
        /// </summary>
        /// <param name="template"><see cref="Template"/> to save</param>
        /// <param name="userId"></param>
        public void SaveTemplate(ITemplate template, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingTemplate, this, new SaveEventArgs<ITemplate>(template)))
                {
                    uow.Commit();
                    return;
                }

                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                repository.AddOrUpdate(template);

                uow.Events.Dispatch(SavedTemplate, this, new SaveEventArgs<ITemplate>(template, false));

                Audit(uow, AuditType.Save, "Save Template performed by user", userId, template.Id);
                uow.Commit();
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="Template"/> objects
        /// </summary>
        /// <param name="templates">List of <see cref="Template"/> to save</param>
        /// <param name="userId">Optional id of the user</param>
        public void SaveTemplate(IEnumerable<ITemplate> templates, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingTemplate, this, new SaveEventArgs<ITemplate>(templates)))
                {
                    uow.Commit();
                    return;
                }
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                foreach (var template in templates)
                {
                    repository.AddOrUpdate(template);
                }

                uow.Events.Dispatch(SavedTemplate, this, new SaveEventArgs<ITemplate>(templates, false));

                Audit(uow, AuditType.Save, "Save Template performed by user", userId, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// This checks what the default rendering engine is set in config but then also ensures that there isn't already
        /// a template that exists in the opposite rendering engine's template folder, then returns the appropriate
        /// rendering engine to use.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The reason this is required is because for example, if you have a master page file already existing under ~/masterpages/Blah.aspx
        /// and then you go to create a template in the tree called Blah and the default rendering engine is MVC, it will create a Blah.cshtml
        /// empty template in ~/Views. This means every page that is using Blah will go to MVC and render an empty page.
        /// This is mostly related to installing packages since packages install file templates to the file system and then create the
        /// templates in business logic. Without this, it could cause the wrong rendering engine to be used for a package.
        /// </remarks>
        public RenderingEngine DetermineTemplateRenderingEngine(ITemplate template)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.DetermineTemplateRenderingEngine(template);
            }
        }

        /// <summary>
        /// Deletes a template by its alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="ITemplate"/> to delete</param>
        /// <param name="userId"></param>
        public void DeleteTemplate(string alias, int userId = 0)
        {
            // v8 - that should be part of the uow / scope
            var template = GetTemplate(alias);
            if (template == null)
                return;

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);

                var args = new DeleteEventArgs<ITemplate>(template);
                
                if (uow.Events.DispatchCancelable(DeletingTemplate, this, args))
                {
                    uow.Commit();
                    return;
                }
                
                repository.Delete(template);

                args.CanCancel = false;
                uow.Events.Dispatch(DeletedTemplate, this, args);
                
                Audit(uow, AuditType.Delete, "Delete Template performed by user", userId, template.Id);
                uow.Commit();
            }
        }

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(ITemplate template)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.ValidateTemplate(template);
            }
        }

        public Stream GetTemplateFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetTemplateFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetTemplateFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                return repository.GetFileSize(filepath);
            }
        }

        public Stream GetMacroScriptFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroScriptRepository(uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetMacroScriptFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateMacroScriptRepository(uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetMacroScriptFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroScriptRepository(uow);
                return repository.GetFileSize(filepath);
            }
        }

        #endregion

        public Stream GetStylesheetFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetStylesheetFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetStylesheetFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateStylesheetRepository(uow);
                return repository.GetFileSize(filepath);
            }
        }

        public Stream GetScriptFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetScriptFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetScriptFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateScriptRepository(uow);
                return repository.GetFileSize(filepath);
            }
        }

        public Stream GetUserControlFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserControlRepository(uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetUserControlFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateUserControlRepository(uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetUserControlFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserControlRepository(uow);
                return repository.GetFileSize(filepath);
            }
        }

        public Stream GetXsltFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateXsltFileRepository(uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetXsltFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateXsltFileRepository(uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetXsltFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateXsltFileRepository(uow);
                return repository.GetFileSize(filepath);
            }
        }

        #region Partial Views

        public IEnumerable<string> GetPartialViewSnippetNames(params string[] filterNames)
        {
            var snippetPath = IOHelper.MapPath(string.Format("{0}/PartialViewMacros/Templates/", SystemDirectories.Umbraco));
            var files = Directory.GetFiles(snippetPath, "*.cshtml")
                .Select(Path.GetFileNameWithoutExtension)
                .Except(filterNames, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            //Ensure the ones that are called 'Empty' are at the top
            var empty = files.Where(x => Path.GetFileName(x).InvariantStartsWith("Empty"))
                .OrderBy(x => x.Length)
                .ToArray();

            return empty.Union(files.Except(empty));
        }

        public void CreatePartialViewFolder(string folderPath)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreatePartialViewRepository(uow);
                ((PartialViewRepository)repository).AddFolder(folderPath);
                uow.Commit();
            }
        }

        public void CreatePartialViewMacroFolder(string folderPath)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreatePartialViewMacroRepository(uow);
                ((PartialViewMacroRepository)repository).AddFolder(folderPath);
                uow.Commit();
            }
        }

        public void DeletePartialViewFolder(string folderPath)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreatePartialViewRepository(uow);
                ((PartialViewRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        public void DeletePartialViewMacroFolder(string folderPath)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreatePartialViewMacroRepository(uow);
                ((PartialViewMacroRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        public IPartialView GetPartialView(string path)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreatePartialViewRepository(uow);
                return repository.Get(path);
            }
        }

        public IPartialView GetPartialViewMacro(string path)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreatePartialViewMacroRepository(uow);
                return repository.Get(path);
            }
        }

        [Obsolete("MacroScripts are obsolete - this is for backwards compatibility with upgraded sites.")]
        public IPartialView GetMacroScript(string path)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroScriptRepository(uow);
                return repository.Get(path);
            }
        }

        [Obsolete("UserControls are obsolete - this is for backwards compatibility with upgraded sites.")]
        public IUserControl GetUserControl(string path)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserControlRepository(uow);
                return repository.Get(path);
            }
        }

        public IEnumerable<IPartialView> GetPartialViewMacros(params string[] names)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreatePartialViewMacroRepository(uow);
                return repository.GetAll(names).OrderBy(x => x.Name);
            }
        }

        public IXsltFile GetXsltFile(string path)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateXsltFileRepository(uow);
                return repository.Get(path);
            }
        }

        public IEnumerable<IXsltFile> GetXsltFiles(params string[] names)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateXsltFileRepository(uow);
                return repository.GetAll(names).OrderBy(x => x.Name);
            }
        }

        public Attempt<IPartialView> CreatePartialView(IPartialView partialView, string snippetName = null, int userId = 0)
        {
            return CreatePartialViewMacro(partialView, PartialViewType.PartialView, snippetName, userId);
        }

        public Attempt<IPartialView> CreatePartialViewMacro(IPartialView partialView, string snippetName = null, int userId = 0)
        {
            return CreatePartialViewMacro(partialView, PartialViewType.PartialViewMacro, snippetName, userId);
        }

        private Attempt<IPartialView> CreatePartialViewMacro(IPartialView partialView, PartialViewType partialViewType, string snippetName = null, int userId = 0)
        {
            var newEventArgs = new NewEventArgs<IPartialView>(partialView, true, partialView.Alias, -1);
            using (var scope = UowProvider.ScopeProvider.CreateScope())
            {
                scope.Complete(); // always                
                if (scope.Events.DispatchCancelable(CreatingPartialView, this, newEventArgs))
                    return Attempt<IPartialView>.Fail();
            }

            if (snippetName.IsNullOrWhiteSpace() == false)
            {
                partialView.Content = GetPartialViewMacroSnippetContent(snippetName, partialViewType);

            }

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = GetPartialViewRepository(partialViewType, uow);
                repository.AddOrUpdate(partialView);
                newEventArgs.CanCancel = false;
                uow.Events.Dispatch(CreatedPartialView, this, newEventArgs);

                Audit(uow, AuditType.Save, string.Format("Save {0} performed by user", partialViewType), userId, -1);
                uow.Commit();
            }

            return Attempt<IPartialView>.Succeed(partialView);
        }

        public bool DeletePartialView(string path, int userId = 0)
        {
            return DeletePartialViewMacro(path, PartialViewType.PartialView, userId);
        }

        public bool DeletePartialViewMacro(string path, int userId = 0)
        {
            return DeletePartialViewMacro(path, PartialViewType.PartialViewMacro, userId);
        }

        private bool DeletePartialViewMacro(string path, PartialViewType partialViewType, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = GetPartialViewRepository(partialViewType, uow);
                var partialView = repository.Get(path);
                if (partialView == null)
                {
                    uow.Commit();
                    return false;
                }

                var deleteEventArgs = new DeleteEventArgs<IPartialView>(partialView);
                if (uow.Events.DispatchCancelable(DeletingPartialView, this, deleteEventArgs))
                {
                    uow.Commit();
                    return false;
                }

                repository.Delete(partialView);
                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedPartialView, this, deleteEventArgs);

                Audit(uow, AuditType.Delete, string.Format("Delete {0} performed by user", partialViewType), userId, -1);
                uow.Commit();
            }

            return true;

        }

        public Attempt<IPartialView> SavePartialView(IPartialView partialView, int userId = 0)
        {
            return SavePartialView(partialView, PartialViewType.PartialView, userId);
        }

        public Attempt<IPartialView> SavePartialViewMacro(IPartialView partialView, int userId = 0)
        {
            return SavePartialView(partialView, PartialViewType.PartialViewMacro, userId);
        }

        private Attempt<IPartialView> SavePartialView(IPartialView partialView, PartialViewType partialViewType, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<IPartialView>(partialView);
                if (uow.Events.DispatchCancelable(SavingPartialView, this, saveEventArgs))
                {
                    uow.Commit();
                    return Attempt<IPartialView>.Fail();
                }

                var repository = GetPartialViewRepository(partialViewType, uow);
                repository.AddOrUpdate(partialView);
                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(SavedPartialView, this, saveEventArgs);

                Audit(uow, AuditType.Save, string.Format("Save {0} performed by user", partialViewType), userId, -1);
                uow.Commit();
            }

            return Attempt.Succeed(partialView);
        }

        public bool ValidatePartialView(PartialView partialView)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreatePartialViewRepository(uow);
                return repository.ValidatePartialView(partialView);
            }
        }

        public bool ValidatePartialViewMacro(PartialView partialView)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreatePartialViewMacroRepository(uow);
                return repository.ValidatePartialView(partialView);
            }
        }

        internal string StripPartialViewHeader(string contents)
        {
            var headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline);
            return headerMatch.Replace(contents, string.Empty);
        }

        internal Attempt<string> TryGetSnippetPath(string fileName)
        {
            if (fileName.EndsWith(".cshtml") == false)
            {
                fileName += ".cshtml";
            }

            var snippetPath = IOHelper.MapPath(string.Format("{0}/PartialViewMacros/Templates/{1}", SystemDirectories.Umbraco, fileName));
            return System.IO.File.Exists(snippetPath)
                ? Attempt<string>.Succeed(snippetPath)
                : Attempt<string>.Fail();
        }

        private IPartialViewRepository GetPartialViewRepository(PartialViewType partialViewType, IUnitOfWork uow)
        {
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    return RepositoryFactory.CreatePartialViewRepository(uow);
                case PartialViewType.PartialViewMacro:
                    return RepositoryFactory.CreatePartialViewMacroRepository(uow);
            }
            throw new ArgumentOutOfRangeException("partialViewType");
        }

        public Stream GetPartialViewMacroFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = GetPartialViewRepository(PartialViewType.PartialViewMacro, uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public string GetPartialViewSnippetContent(string snippetName)
        {
            return GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialView);
        }

        public string GetPartialViewMacroSnippetContent(string snippetName)
        {
            return GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialViewMacro);
        }

        private string GetViewContent(string filename)
        {
            if (filename.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(filename));

            if (filename.EndsWith(".cshtml") == false)
            {
                filename = $"{filename}.cshtml";
            }

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateTemplateRepository(uow);
                var stream = repository.GetFileContentStream(filename);

                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
        }

        private string GetPartialViewMacroSnippetContent(string snippetName, PartialViewType partialViewType)
        {
            if (snippetName.IsNullOrWhiteSpace())
                throw new ArgumentNullException("snippetName");

            string partialViewHeader;
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    partialViewHeader = PartialViewHeader;
                    break;
                case PartialViewType.PartialViewMacro:
                    partialViewHeader = PartialViewMacroHeader;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("partialViewType");
            }

            // Try and get the snippet path
            var snippetPathAttempt = TryGetSnippetPath(snippetName);
            if (snippetPathAttempt.Success == false)
            {
                throw new InvalidOperationException("Could not load snippet with name " + snippetName);
            }

            using (var snippetFile = new StreamReader(System.IO.File.OpenRead(snippetPathAttempt.Result)))
            {
                var snippetContent = snippetFile.ReadToEnd().Trim();

                //strip the @inherits if it's there
                snippetContent = StripPartialViewHeader(snippetContent);

                var content = string.Format("{0}{1}{2}",
                    partialViewHeader,
                    Environment.NewLine, snippetContent);
                return content;
            }
        }

        public void SetPartialViewMacroFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = GetPartialViewRepository(PartialViewType.PartialViewMacro, uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetPartialViewMacroFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = GetPartialViewRepository(PartialViewType.PartialViewMacro, uow);
                return repository.GetFileSize(filepath);
            }
        }

        public Stream GetPartialViewFileContentStream(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = GetPartialViewRepository(PartialViewType.PartialView, uow);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetPartialViewFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = GetPartialViewRepository(PartialViewType.PartialView, uow);
                repository.SetFileContent(filepath, content);
                uow.Commit();
            }
        }

        public long GetPartialViewFileSize(string filepath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = GetPartialViewRepository(PartialViewType.PartialView, uow);
                return repository.GetFileSize(filepath);
            }
        }

        #endregion

        private void Audit(IScopeUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var repository = RepositoryFactory.CreateAuditRepository(uow);
            repository.AddOrUpdate(new AuditItem(objectId, message, type, userId));
        }

        //TODO Method to change name and/or alias of view/masterpage template

        #region Event Handlers
        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<ITemplate>> DeletingTemplate;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<ITemplate>> DeletedTemplate;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Script>> DeletingScript;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Script>> DeletedScript;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Stylesheet>> DeletingStylesheet;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Stylesheet>> DeletedStylesheet;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<ITemplate>> SavingTemplate;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<ITemplate>> SavedTemplate;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Script>> SavingScript;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Script>> SavedScript;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Stylesheet>> SavingStylesheet;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Stylesheet>> SavedStylesheet;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<IPartialView>> SavingPartialView;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<IPartialView>> SavedPartialView;

        /// <summary>
        /// Occurs before Create
        /// </summary>
        public static event TypedEventHandler<IFileService, NewEventArgs<IPartialView>> CreatingPartialView;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        public static event TypedEventHandler<IFileService, NewEventArgs<IPartialView>> CreatedPartialView;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<IPartialView>> DeletingPartialView;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<IPartialView>> DeletedPartialView;

        #endregion
    }
}
