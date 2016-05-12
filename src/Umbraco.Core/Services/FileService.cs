using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
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
    public class FileService : RepositoryService, IFileService
    {
        private readonly IUnitOfWorkProvider _fileUowProvider;

        private const string PartialViewHeader = "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage";
        private const string PartialViewMacroHeader = "@inherits Umbraco.Web.Macros.PartialViewMacroPage";

        public FileService(
            IUnitOfWorkProvider fileProvider,
            IDatabaseUnitOfWorkProvider dataProvider,
            ILogger logger,
            IEventMessagesFactory eventMessagesFactory)
            : base(dataProvider, logger, eventMessagesFactory)
        {
            _fileUowProvider = fileProvider;
        }


        #region Stylesheets

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        public IEnumerable<Stylesheet> GetStylesheets(params string[] names)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                var stylesheets = repository.GetAll(names);
                uow.Complete();
                return stylesheets;
            }
        }

        /// <summary>
        /// Gets a <see cref="Stylesheet"/> object by its name
        /// </summary>
        /// <param name="name">Name of the stylesheet incl. extension</param>
        /// <returns>A <see cref="Stylesheet"/> object</returns>
        public Stylesheet GetStylesheetByName(string name)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                var stylesheet = repository.Get(name);
                uow.Complete();
                return stylesheet;
            }
        }

        /// <summary>
        /// Saves a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to save</param>
        /// <param name="userId"></param>
        public void SaveStylesheet(Stylesheet stylesheet, int userId = 0)
        {
            if (SavingStylesheet.IsRaisedEventCancelled(new SaveEventArgs<Stylesheet>(stylesheet), this))
                return;

            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                repository.AddOrUpdate(stylesheet);
                uow.Complete();
            }

            SavedStylesheet.RaiseEvent(new SaveEventArgs<Stylesheet>(stylesheet, false), this);
            Audit(AuditType.Save, "Save Stylesheet performed by user", userId, -1);
        }

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Stylesheet to delete</param>
        /// <param name="userId"></param>
        public void DeleteStylesheet(string path, int userId = 0)
        {
            Stylesheet stylesheet;
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                stylesheet = repository.Get(path);
                if (stylesheet == null)
                {
                    uow.Complete();
                    return;
                }

                if (DeletingStylesheet.IsRaisedEventCancelled(new DeleteEventArgs<Stylesheet>(stylesheet), this))
                    return; // causes rollback

                repository.Delete(stylesheet);
                uow.Complete();
            }

            DeletedStylesheet.RaiseEvent(new DeleteEventArgs<Stylesheet>(stylesheet, false), this);
            Audit(AuditType.Delete, "Delete Stylesheet performed by user", userId, -1);
        }

        /// <summary>
        /// Validates a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to validate</param>
        /// <returns>True if Stylesheet is valid, otherwise false</returns>
        public bool ValidateStylesheet(Stylesheet stylesheet)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IStylesheetRepository>();
                var valid = repository.ValidateStylesheet(stylesheet);
                uow.Complete();
                return valid;
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
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                var scripts = repository.GetAll(names);
                uow.Complete();
                return scripts;
            }
        }

        /// <summary>
        /// Gets a <see cref="Script"/> object by its name
        /// </summary>
        /// <param name="name">Name of the script incl. extension</param>
        /// <returns>A <see cref="Script"/> object</returns>
        public Script GetScriptByName(string name)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                var script = repository.Get(name);
                uow.Complete();
                return script;
            }
        }

        /// <summary>
        /// Saves a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to save</param>
        /// <param name="userId"></param>
        public void SaveScript(Script script, int userId = 0)
        {
            if (SavingScript.IsRaisedEventCancelled(new SaveEventArgs<Script>(script), this))
                return;

            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                repository.AddOrUpdate(script);
                uow.Complete();
            }

            SavedScript.RaiseEvent(new SaveEventArgs<Script>(script, false), this);
            Audit(AuditType.Save, string.Format("Save Script performed by user"), userId, -1);
        }

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Script to delete</param>
        /// <param name="userId"></param>
        public void DeleteScript(string path, int userId = 0)
        {
            Script script;

            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                script = repository.Get(path);
                if (script == null)
                {
                    uow.Complete();
                    return;
                }

                if (DeletingScript.IsRaisedEventCancelled(new DeleteEventArgs<Script>(script), this))
                    return; // causes rollback

                repository.Delete(script);
                uow.Complete();
            }

            DeletedScript.RaiseEvent(new DeleteEventArgs<Script>(script, false), this);
            Audit(AuditType.Delete, string.Format("Delete Script performed by user"), userId, -1);
        }

        /// <summary>
        /// Validates a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateScript(Script script)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                var valid = repository.ValidateScript(script);
                uow.Complete();
                return valid;
            }
        }

        public void CreateScriptFolder(string folderPath)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                ((ScriptRepository) repository).AddFolder(folderPath);
                uow.Complete();
            }
        }

        public void DeleteScriptFolder(string folderPath)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IScriptRepository>();
                ((ScriptRepository) repository).DeleteFolder(folderPath);
                uow.Complete();
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
            if (SavingTemplate.IsRaisedEventCancelled(
                  new SaveEventArgs<ITemplate>(template, true, evtMsgs, additionalData),
                  this))
            {
                return OperationStatus.Attempt.Fail<OperationStatusType, ITemplate>(OperationStatusType.FailedCancelledByEvent, evtMsgs, template);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                repository.AddOrUpdate(template);
                uow.Complete();
            }

            SavedTemplate.RaiseEvent(new SaveEventArgs<ITemplate>(template, false, evtMsgs), this);
            Audit(AuditType.Save, "Save Template performed by user", userId, template.Id);

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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var templates = repository.GetAll(aliases).OrderBy(x => x.Name);
                uow.Complete();
                return templates;
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(int masterTemplateId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var templates = repository.GetChildren(masterTemplateId).OrderBy(x => x.Name);
                uow.Complete();
                return templates;
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias.
        /// </summary>
        /// <param name="alias">The alias of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the alias, or null.</returns>
        public ITemplate GetTemplate(string alias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var template = repository.Get(alias);
                uow.Complete();
                return template;
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its identifier.
        /// </summary>
        /// <param name="id">The identifer of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        public ITemplate GetTemplate(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var template = repository.Get(id);
                uow.Complete();
                return template;
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its guid identifier.
        /// </summary>
        /// <param name="id">The guid identifier of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        public ITemplate GetTemplate(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var query = repository.Query.Where(x => x.Key == id);
                var template = repository.GetByQuery(query).SingleOrDefault();
                uow.Complete();
                return template;
            }
        }

        public IEnumerable<ITemplate> GetTemplateDescendants(string alias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var templates = repository.GetDescendants(alias);
                uow.Complete();
                return templates;
            }
        }

        /// <summary>
        /// Gets the template descendants
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateDescendants(int masterTemplateId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var templates = repository.GetDescendants(masterTemplateId);
                uow.Complete();
                return templates;
            }
        }

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateChildren(string alias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var templates = repository.GetChildren(alias);
                uow.Complete();
                return templates;
            }
        }

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateChildren(int masterTemplateId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var templates = repository.GetChildren(masterTemplateId);
                uow.Complete();
                return templates;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var template = repository.GetTemplateNode(alias);
                uow.Complete();
                return template;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var template = repository.FindTemplateInTree(anyNode, alias);
                uow.Complete();
                return template;
            }
        }

        /// <summary>
        /// Saves a <see cref="Template"/>
        /// </summary>
        /// <param name="template"><see cref="Template"/> to save</param>
        /// <param name="userId"></param>
        public void SaveTemplate(ITemplate template, int userId = 0)
        {
            if (SavingTemplate.IsRaisedEventCancelled(new SaveEventArgs<ITemplate>(template), this))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                repository.AddOrUpdate(template);
                uow.Complete();
            }

            SavedTemplate.RaiseEvent(new SaveEventArgs<ITemplate>(template, false), this);
            Audit(AuditType.Save, "Save Template performed by user", userId, template.Id);
        }

        /// <summary>
        /// Saves a collection of <see cref="Template"/> objects
        /// </summary>
        /// <param name="templates">List of <see cref="Template"/> to save</param>
        /// <param name="userId">Optional id of the user</param>
        public void SaveTemplate(IEnumerable<ITemplate> templates, int userId = 0)
        {
            if (SavingTemplate.IsRaisedEventCancelled(new SaveEventArgs<ITemplate>(templates), this))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                foreach (var template in templates)
                    repository.AddOrUpdate(template);
                uow.Complete();
            }

            SavedTemplate.RaiseEvent(new SaveEventArgs<ITemplate>(templates, false), this);
            Audit(AuditType.Save, "Save Template performed by user", userId, -1);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var engine = repository.DetermineTemplateRenderingEngine(template);
                uow.Complete();
                return engine;
            }
        }

        /// <summary>
        /// Deletes a template by its alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="ITemplate"/> to delete</param>
        /// <param name="userId"></param>
        public void DeleteTemplate(string alias, int userId = 0)
        {
            ITemplate template;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                template = repository.Get(alias);
                if (template == null)
                {
                    uow.Complete();
                    return;
                }

                if (DeletingTemplate.IsRaisedEventCancelled(new DeleteEventArgs<ITemplate>(template), this))
                    return; // causes rollback

                repository.Delete(template);
                uow.Complete();
            }

            DeletedTemplate.RaiseEvent(new DeleteEventArgs<ITemplate>(template, false), this);
            Audit(AuditType.Delete, "Delete Template performed by user", userId, template.Id);
        }

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(ITemplate template)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<ITemplateRepository>();
                var valid = repository.ValidateTemplate(template);
                uow.Complete();
                return valid;
            }
        }

        #endregion

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

        public void DeletePartialViewFolder(string folderPath)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewRepository>();
                ((PartialViewRepository) repository).DeleteFolder(folderPath);
                uow.Complete();
            }
        }

        public void DeletePartialViewMacroFolder(string folderPath)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewMacroRepository>();
                ((PartialViewMacroRepository) repository).DeleteFolder(folderPath);
                uow.Complete();
            }
        }

        public IPartialView GetPartialView(string path)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewRepository>();
                var view = repository.Get(path);
                uow.Complete();
                return view;
            }
        }

        public IPartialView GetPartialViewMacro(string path)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewMacroRepository>();
                var view = repository.Get(path);
                uow.Complete();
                return view;
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
            if (CreatingPartialView.IsRaisedEventCancelled(new NewEventArgs<IPartialView>(partialView, true, partialView.Alias, -1), this))
                return Attempt<IPartialView>.Fail();

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

                    var content = string.Format("{0}{1}{2}",
                        partialViewHeader,
                        Environment.NewLine, snippetContent);
                    partialView.Content = content;
                }
            }

            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(partialViewType);
                repository.AddOrUpdate(partialView);
                uow.Complete();
            }

            CreatedPartialView.RaiseEvent(new NewEventArgs<IPartialView>(partialView, false, partialView.Alias, -1), this);
            Audit(AuditType.Save, $"Save {partialViewType} performed by user", userId, -1);
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
            IPartialView partialView;
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(partialViewType);
                partialView = repository.Get(path);
                if (partialView == null)
                {
                    uow.Complete();
                    return true;
                }

                if (DeletingPartialView.IsRaisedEventCancelled(new DeleteEventArgs<IPartialView>(partialView), this))
                    return false; // causes rollback

                repository.Delete(partialView);
                uow.Complete();
            }

            DeletedPartialView.RaiseEvent(new DeleteEventArgs<IPartialView>(partialView, false), this);
            Audit(AuditType.Delete, $"Delete {partialViewType} performed by user", userId, -1);
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
            if (SavingPartialView.IsRaisedEventCancelled(new SaveEventArgs<IPartialView>(partialView), this))
                return Attempt<IPartialView>.Fail();

            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreatePartialViewRepository(partialViewType);
                repository.AddOrUpdate(partialView);
                uow.Complete();
            }

            Audit(AuditType.Save, $"Save {partialViewType} performed by user", userId, -1);
            SavedPartialView.RaiseEvent(new SaveEventArgs<IPartialView>(partialView, false), this);
            return Attempt.Succeed(partialView);
        }

        public bool ValidatePartialView(PartialView partialView)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewRepository>();
                var valid = repository.ValidatePartialView(partialView);
                uow.Complete();
                return valid;
            }
        }

        public bool ValidatePartialViewMacro(PartialView partialView)
        {
            using (var uow = _fileUowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IPartialViewMacroRepository>();
                var valid = repository.ValidatePartialView(partialView);
                uow.Complete();
                return valid;
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

        #endregion

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Complete();
            }
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