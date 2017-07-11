using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
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

        public FileService(IScopeUnitOfWorkProvider uowProvider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(uowProvider, logger, eventMessagesFactory)
        { }

        #region Stylesheets

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        public IEnumerable<Stylesheet> GetStylesheets(params string[] names)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingStylesheet, this, new SaveEventArgs<Stylesheet>(stylesheet)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IStylesheetRepository>();
                repository.AddOrUpdate(stylesheet);

                uow.Events.Dispatch(SavedStylesheet, this, new SaveEventArgs<Stylesheet>(stylesheet, false));

                Audit(uow, AuditType.Save, "Save Stylesheet performed by user", userId, -1);
                uow.Complete();
            }
        }

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Stylesheet to delete</param>
        /// <param name="userId"></param>
        public void DeleteStylesheet(string path, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                var stylesheet = repository.Get(path);
                if (stylesheet == null)
                {
                    uow.Complete();
                    return;
                }

                if (uow.Events.DispatchCancelable(DeletingStylesheet, this, new DeleteEventArgs<Stylesheet>(stylesheet)))
                {
                    uow.Complete();
                    return; // causes rollback
                }

                repository.Delete(stylesheet);

                uow.Events.Dispatch(DeletedStylesheet, this, new DeleteEventArgs<Stylesheet>(stylesheet, false));

                Audit(uow, AuditType.Delete, "Delete Stylesheet performed by user", userId, -1);
                uow.Complete();
            }
        }

        /// <summary>
        /// Validates a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to validate</param>
        /// <returns>True if Stylesheet is valid, otherwise false</returns>
        public bool ValidateStylesheet(Stylesheet stylesheet)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                return repository.ValidateStylesheet(stylesheet);
            }
        }

        public Stream GetStylesheetFileContentStream(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetStylesheetFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                repository.SetFileContent(filepath, content);
                uow.Complete();
            }
        }

        public long GetStylesheetFileSize(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                return repository.GetFileSize(filepath);
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IScriptRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IScriptRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingScript, this, new SaveEventArgs<Script>(script)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IScriptRepository>();
                repository.AddOrUpdate(script);

                uow.Events.Dispatch(SavedScript, this, new SaveEventArgs<Script>(script, false));

                Audit(uow, AuditType.Save, "Save Script performed by user", userId, -1);
                uow.Complete();
            }
        }

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Script to delete</param>
        /// <param name="userId"></param>
        public void DeleteScript(string path, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                var script = repository.Get(path);
                if (script == null)
                {
                    uow.Complete();
                    return;
                }

                if (uow.Events.DispatchCancelable(DeletingScript, this, new DeleteEventArgs<Script>(script)))
                {
                    uow.Complete();
                    return;
                }

                repository.Delete(script);

                uow.Events.Dispatch(DeletedScript, this, new DeleteEventArgs<Script>(script, false));

                Audit(uow, AuditType.Delete, "Delete Script performed by user", userId, -1);
                uow.Complete();
            }
        }

        /// <summary>
        /// Validates a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateScript(Script script)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                return repository.ValidateScript(script);
            }
        }

        public void CreateScriptFolder(string folderPath)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                ((ScriptRepository) repository).AddFolder(folderPath);
                uow.Complete();
            }
        }

        public void DeleteScriptFolder(string folderPath)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                ((ScriptRepository) repository).DeleteFolder(folderPath);
                uow.Complete();
            }
        }

        public Stream GetScriptFileContentStream(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetScriptFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                repository.SetFileContent(filepath, content);
                uow.Complete();
            }
        }

        public long GetScriptFileSize(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                return repository.GetFileSize(filepath);
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
        public Attempt<OperationStatus<OperationStatusType, ITemplate>> CreateTemplateForContentType(string contentTypeAlias, string contentTypeName, int userId = 0)
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingTemplate, this, new SaveEventArgs<ITemplate>(template, true, evtMsgs, additionalData)))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Fail<OperationStatusType, ITemplate>(OperationStatusType.FailedCancelledByEvent, evtMsgs, template);
                }

                var repository = uow.CreateRepository<ITemplateRepository>();
                repository.AddOrUpdate(template);

                uow.Events.Dispatch(SavedTemplate, this, new SaveEventArgs<ITemplate>(template, false, evtMsgs));

                Audit(uow, AuditType.Save, "Save Template performed by user", userId, template.Id);
                uow.Complete();
            }

            return OperationStatus.Attempt.Succeed<OperationStatusType, ITemplate>(OperationStatusType.Success, evtMsgs, template);
        }

        public ITemplate CreateTemplateWithIdentity(string name, string content, ITemplate masterTemplate = null, int userId = 0)
        {
            var template = new Template(name, name)
            {
                Content = content
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                return repository.GetAll(aliases).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(int masterTemplateId)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var query = uow.Query<ITemplate>().Where(x => x.Key == id);
                return repository.GetByQuery(query).SingleOrDefault();
            }
        }

        public IEnumerable<ITemplate> GetTemplateDescendants(string alias)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingTemplate, this, new SaveEventArgs<ITemplate>(template)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<ITemplateRepository>();
                repository.AddOrUpdate(template);

                uow.Events.Dispatch(SavedTemplate, this, new SaveEventArgs<ITemplate>(template, false));

                Audit(uow, AuditType.Save, "Save Template performed by user", userId, template.Id);
                uow.Complete();
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="Template"/> objects
        /// </summary>
        /// <param name="templates">List of <see cref="Template"/> to save</param>
        /// <param name="userId">Optional id of the user</param>
        public void SaveTemplate(IEnumerable<ITemplate> templates, int userId = 0)
        {
            var templatesA = templates.ToArray();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingTemplate, this, new SaveEventArgs<ITemplate>(templatesA)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<ITemplateRepository>();
                foreach (var template in templatesA)
                    repository.AddOrUpdate(template);

                uow.Events.Dispatch(SavedTemplate, this, new SaveEventArgs<ITemplate>(templatesA, false));

                Audit(uow, AuditType.Save, "Save Template performed by user", userId, -1);
                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var template = repository.Get(alias);
                if (template == null)
                {
                    uow.Complete();
                    return;
                }

                if (uow.Events.DispatchCancelable(DeletingTemplate, this, new DeleteEventArgs<ITemplate>(template)))
                {
                    uow.Complete();
                    return;
                }

                repository.Delete(template);

                uow.Events.Dispatch(DeletedTemplate, this, new DeleteEventArgs<ITemplate>(template, false));

                Audit(uow, AuditType.Delete, "Delete Template performed by user", userId, template.Id);
                uow.Complete();
            }
        }

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(ITemplate template)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                return repository.ValidateTemplate(template);
            }
        }

        public Stream GetTemplateFileContentStream(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetTemplateFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                repository.SetFileContent(filepath, content);
                uow.Complete();
            }
        }

        public long GetTemplateFileSize(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                return repository.GetFileSize(filepath);
            }
        }

        #endregion

        #region Partial Views

        public IEnumerable<string> GetPartialViewSnippetNames(params string[] filterNames)
        {
            var snippetPath = IOHelper.MapPath($"{SystemDirectories.Umbraco}/PartialViewMacros/Templates/");
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

        public void DeletePartialViewFolder(string folderPath)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewRepository>();
                ((PartialViewRepository) repository).DeleteFolder(folderPath);
                uow.Complete();
            }
        }

        public void DeletePartialViewMacroFolder(string folderPath)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewMacroRepository>();
                ((PartialViewMacroRepository) repository).DeleteFolder(folderPath);
                uow.Complete();
            }
        }

        public IPartialView GetPartialView(string path)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IPartialViewRepository>();
                return repository.Get(path);
            }
        }

        public IPartialView GetPartialViewMacro(string path)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IPartialViewMacroRepository>();
                return repository.Get(path);
            }
        }

        public IEnumerable<IPartialView> GetPartialViewMacros(params string[] names)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IPartialViewMacroRepository>();
                return repository.GetAll(names).OrderBy(x => x.Name);
            }
        }

        public IXsltFile GetXsltFile(string path)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IXsltFileRepository>();
                return repository.Get(path);
            }
        }

        public IEnumerable<IXsltFile> GetXsltFiles(params string[] names)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IXsltFileRepository>();
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
                    throw new ArgumentOutOfRangeException(nameof(partialViewType));
            }

            string partialViewContent = null;
            if (snippetName.IsNullOrWhiteSpace() == false)
            {
                //create the file
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

                    partialViewContent = $"{partialViewHeader}{Environment.NewLine}{snippetContent}";
                }
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(CreatingPartialView, this, new NewEventArgs<IPartialView>(partialView, true, partialView.Alias, -1)))
                {
                    uow.Complete();
                    return Attempt<IPartialView>.Fail();
                }

                var repository = uow.CreatePartialViewRepository(partialViewType);
                if (partialViewContent != null) partialView.Content = partialViewContent;
                repository.AddOrUpdate(partialView);

                uow.Events.Dispatch(CreatedPartialView, this, new NewEventArgs<IPartialView>(partialView, false, partialView.Alias, -1));
                Audit(uow, AuditType.Save, $"Save {partialViewType} performed by user", userId, -1);

                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(partialViewType);
                var partialView = repository.Get(path);
                if (partialView == null)
                {
                    uow.Complete();
                    return true;
                }

                if (uow.Events.DispatchCancelable(DeletingPartialView, this, new DeleteEventArgs<IPartialView>(partialView)))
                {
                    uow.Complete();
                    return false; // causes rollback
                }

                repository.Delete(partialView);

                uow.Events.Dispatch(DeletedPartialView, this, new DeleteEventArgs<IPartialView>(partialView, false));
                Audit(uow, AuditType.Delete, $"Delete {partialViewType} performed by user", userId, -1);

                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingPartialView, this, new SaveEventArgs<IPartialView>(partialView)))
                {
                    uow.Complete();
                    return Attempt<IPartialView>.Fail();
                }

                var repository = uow.CreatePartialViewRepository(partialViewType);
                repository.AddOrUpdate(partialView);

                Audit(uow, AuditType.Save, $"Save {partialViewType} performed by user", userId, -1);
                uow.Events.Dispatch(SavedPartialView, this, new SaveEventArgs<IPartialView>(partialView, false));

                uow.Complete();
            }

            return Attempt.Succeed(partialView);
        }

        public bool ValidatePartialView(PartialView partialView)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IPartialViewRepository>();
                return repository.ValidatePartialView(partialView);
            }
        }

        public bool ValidatePartialViewMacro(PartialView partialView)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IPartialViewMacroRepository>();
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

            var snippetPath = IOHelper.MapPath($"{SystemDirectories.Umbraco}/PartialViewMacros/Templates/{fileName}");
            return System.IO.File.Exists(snippetPath)
                ? Attempt<string>.Succeed(snippetPath)
                : Attempt<string>.Fail();
        }

        public Stream GetPartialViewMacroFileContentStream(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialViewMacro);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetPartialViewMacroFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialViewMacro);
                repository.SetFileContent(filepath, content);
                uow.Complete();
            }
        }

        public Stream GetPartialViewFileContentStream(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialView);
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetPartialViewFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialView);
                repository.SetFileContent(filepath, content);
                uow.Complete();
            }
        }

        public void CreatePartialViewFolder(string folderPath)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialView);
                ((PartialViewRepository) repository).AddFolder(folderPath);
                uow.Complete();
            }
        }

        public void CreatePartialViewMacroFolder(string folderPath)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialViewMacro);
                ((PartialViewMacroRepository) repository).AddFolder(folderPath);
                uow.Complete();
            }
        }

        public long GetPartialViewMacroFileSize(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialViewMacro);
                return repository.GetFileSize(filepath);
            }
        }

        public long GetPartialViewFileSize(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreatePartialViewRepository(PartialViewType.PartialView);
                return repository.GetFileSize(filepath);
            }
        }

        #endregion

        #region Snippets

        public string GetPartialViewSnippetContent(string snippetName)
        {
            return GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialView);
        }

        public string GetPartialViewMacroSnippetContent(string snippetName)
        {
            return GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialViewMacro);
        }

        private string GetPartialViewMacroSnippetContent(string snippetName, PartialViewType partialViewType)
        {
            if (snippetName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(snippetName));

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
                    throw new ArgumentOutOfRangeException(nameof(partialViewType));
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

                var content = $"{partialViewHeader}{Environment.NewLine}{snippetContent}";
                return content;
            }
        }

        #endregion

        #region Xslt

        public Stream GetXsltFileContentStream(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IXsltFileRepository>();
                return repository.GetFileContentStream(filepath);
            }
        }

        public void SetXsltFileContent(string filepath, Stream content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IXsltFileRepository>();
                repository.SetFileContent(filepath, content);
                uow.Complete();
            }
        }

        public long GetXsltFileSize(string filepath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IXsltFileRepository>();
                return repository.GetFileSize(filepath);
            }
        }

        #endregion

        private void Audit(IUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var repo = uow.CreateRepository<IAuditRepository>();
            repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
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