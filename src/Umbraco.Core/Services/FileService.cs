using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.Auditing;
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
    public class FileService : IFileService
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkProvider _fileUowProvider;
        private readonly IDatabaseUnitOfWorkProvider _dataUowProvider;

        private const string PartialViewHeader = "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage";
        private const string PartialViewMacroHeader = "@inherits Umbraco.Web.Macros.PartialViewMacroPage";

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public FileService()
            : this(new RepositoryFactory())
        { }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public FileService(RepositoryFactory repositoryFactory)
            : this(new FileUnitOfWorkProvider(), new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        public FileService(IUnitOfWorkProvider fileProvider, IDatabaseUnitOfWorkProvider dataProvider, RepositoryFactory repositoryFactory)
        {
            if (fileProvider == null) throw new ArgumentNullException("fileProvider");
            if (dataProvider == null) throw new ArgumentNullException("dataProvider");
            if (repositoryFactory == null) throw new ArgumentNullException("repositoryFactory");
            _repositoryFactory = repositoryFactory;
            _fileUowProvider = fileProvider;
            _dataUowProvider = dataProvider;
        }


        #region Stylesheets

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        public IEnumerable<Stylesheet> GetStylesheets(params string[] names)
        {
            using (var repository = _repositoryFactory.CreateStylesheetRepository(_fileUowProvider.GetUnitOfWork(), _dataUowProvider.GetUnitOfWork()))
            {
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
            using (var repository = _repositoryFactory.CreateStylesheetRepository(_fileUowProvider.GetUnitOfWork(), _dataUowProvider.GetUnitOfWork()))
            {
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
            if (SavingStylesheet.IsRaisedEventCancelled(new SaveEventArgs<Stylesheet>(stylesheet), this))
                return;

            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateStylesheetRepository(uow, _dataUowProvider.GetUnitOfWork()))
            {
                repository.AddOrUpdate(stylesheet);
                uow.Commit();

                SavedStylesheet.RaiseEvent(new SaveEventArgs<Stylesheet>(stylesheet, false), this);
            }

            Audit(AuditType.Save, string.Format("Save Stylesheet performed by user"), userId, -1);
        }

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Stylesheet to delete</param>
        /// <param name="userId"></param>
        public void DeleteStylesheet(string path, int userId = 0)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateStylesheetRepository(uow, _dataUowProvider.GetUnitOfWork()))
            {
                var stylesheet = repository.Get(path);
                if (stylesheet == null) return;

                if (DeletingStylesheet.IsRaisedEventCancelled(new DeleteEventArgs<Stylesheet>(stylesheet), this))
                    return;

                repository.Delete(stylesheet);
                uow.Commit();

                DeletedStylesheet.RaiseEvent(new DeleteEventArgs<Stylesheet>(stylesheet, false), this);

                Audit(AuditType.Delete, string.Format("Delete Stylesheet performed by user"), userId, -1);
            }
        }

        /// <summary>
        /// Validates a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to validate</param>
        /// <returns>True if Stylesheet is valid, otherwise false</returns>
        public bool ValidateStylesheet(Stylesheet stylesheet)
        {

            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateStylesheetRepository(uow, _dataUowProvider.GetUnitOfWork()))
            {
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
            using (var repository = _repositoryFactory.CreateScriptRepository(_fileUowProvider.GetUnitOfWork()))
            {
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
            using (var repository = _repositoryFactory.CreateScriptRepository(_fileUowProvider.GetUnitOfWork()))
            {
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
            if (SavingScript.IsRaisedEventCancelled(new SaveEventArgs<Script>(script), this))
                return;

            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateScriptRepository(uow))
            {
                repository.AddOrUpdate(script);
                uow.Commit();

                SavedScript.RaiseEvent(new SaveEventArgs<Script>(script, false), this);
            }

            Audit(AuditType.Save, string.Format("Save Script performed by user"), userId, -1);
        }

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Script to delete</param>
        /// <param name="userId"></param>
        public void DeleteScript(string path, int userId = 0)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateScriptRepository(uow))
            {
                var script = repository.Get(path);
                if (script == null) return;

                if (DeletingScript.IsRaisedEventCancelled(new DeleteEventArgs<Script>(script), this))
                    return; ;

                repository.Delete(script);
                uow.Commit();

                DeletedScript.RaiseEvent(new DeleteEventArgs<Script>(script, false), this);

                Audit(AuditType.Delete, string.Format("Delete Script performed by user"), userId, -1);
            }
        }

        /// <summary>
        /// Validates a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateScript(Script script)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateScriptRepository(uow))
            {
                return repository.ValidateScript(script);
            }
        }

        public void CreateScriptFolder(string folderPath)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateScriptRepository(uow))
            {
                ((ScriptRepository)repository).AddFolder(folderPath);
                uow.Commit();
            }
        }

        public void DeleteScriptFolder(string folderPath)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateScriptRepository(uow))
            {
                ((ScriptRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        #endregion


        #region Templates

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
            using (var repository = _repositoryFactory.CreateTemplateRepository(_dataUowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(aliases).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(int masterTemplateId)
        {
            using (var repository = _repositoryFactory.CreateTemplateRepository(_dataUowProvider.GetUnitOfWork()))
            {
                return repository.GetChildren(masterTemplateId).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias of the template</param>
        /// <returns>A <see cref="Template"/> object</returns>
        public ITemplate GetTemplate(string alias)
        {
            using (var repository = _repositoryFactory.CreateTemplateRepository(_dataUowProvider.GetUnitOfWork()))
            {
                return repository.Get(alias);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias
        /// </summary>
        /// <param name="id">Id of the template</param>
        /// <returns>A <see cref="ITemplate"/> object</returns>
        public ITemplate GetTemplate(int id)
        {
            using (var repository = _repositoryFactory.CreateTemplateRepository(_dataUowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Returns a template as a template node which can be traversed (parent, children)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public TemplateNode GetTemplateNode(string alias)
        {
            using (var repository = _repositoryFactory.CreateTemplateRepository(_dataUowProvider.GetUnitOfWork()))
            {
                return repository.GetTemplateNode(alias);
            }
        }

        /// <summary>
        /// Given a template node in a tree, this will find the template node with the given alias if it is found in the hierarchy, otherwise null
        /// </summary>
        /// <param name="anyNode"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public TemplateNode FindTemplateInTree(TemplateNode anyNode, string alias)
        {
            using (var repository = _repositoryFactory.CreateTemplateRepository(_dataUowProvider.GetUnitOfWork()))
            {
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
            if (SavingTemplate.IsRaisedEventCancelled(new SaveEventArgs<ITemplate>(template), this))
                return;

            var uow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateTemplateRepository(uow))
            {
                repository.AddOrUpdate(template);
                uow.Commit();

                SavedTemplate.RaiseEvent(new SaveEventArgs<ITemplate>(template, false), this);
            }

            Audit(AuditType.Save, string.Format("Save Template performed by user"), userId, template.Id);
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

            var uow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateTemplateRepository(uow))
            {
                foreach (var template in templates)
                {
                    repository.AddOrUpdate(template);
                }
                uow.Commit();

                SavedTemplate.RaiseEvent(new SaveEventArgs<ITemplate>(templates, false), this);
            }

            Audit(AuditType.Save, string.Format("Save Template performed by user"), userId, -1);
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
            var uow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateTemplateRepository(uow))
            {
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
            var uow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateTemplateRepository(uow))
            {
                var template = repository.Get(alias);
                if (template == null) return;

                if (DeletingTemplate.IsRaisedEventCancelled(new DeleteEventArgs<ITemplate>(template), this))
                    return;

                repository.Delete(template);
                uow.Commit();

                DeletedTemplate.RaiseEvent(new DeleteEventArgs<ITemplate>(template, false), this);

                Audit(AuditType.Delete, string.Format("Delete Template performed by user"), userId, template.Id);
            }
        }

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(ITemplate template)
        {
            var uow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateTemplateRepository(uow))
            {
                return repository.ValidateTemplate(template);
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
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewRepository(uow))
            {
                ((PartialViewRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        public void DeletePartialViewMacroFolder(string folderPath)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewMacroRepository(uow))
            {
                ((PartialViewMacroRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        public IPartialView GetPartialView(string path)
        {
            using (var repository = _repositoryFactory.CreatePartialViewRepository(_fileUowProvider.GetUnitOfWork()))
            {
                return repository.Get(path);
            }
        }

        public IPartialView GetPartialViewMacro(string path)
        {
            using (var repository = _repositoryFactory.CreatePartialViewMacroRepository(_fileUowProvider.GetUnitOfWork()))
            {
                return repository.Get(path);
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

            var uow = _fileUowProvider.GetUnitOfWork();
            string partialViewHeader = null;
            IPartialViewRepository repository;
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    repository = _repositoryFactory.CreatePartialViewRepository(uow);
                    partialViewHeader = PartialViewHeader;
                    break;
                case PartialViewType.PartialViewMacro:
                default:
                    repository = _repositoryFactory.CreatePartialViewMacroRepository(uow);
                    partialViewHeader = PartialViewMacroHeader;
                    break;
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

            using (repository)
            {
                repository.AddOrUpdate(partialView);
                uow.Commit();

                CreatedPartialView.RaiseEvent(new NewEventArgs<IPartialView>(partialView, false, partialView.Alias, -1), this);
            }

            Audit(AuditType.Save, string.Format("Save {0} performed by user", partialViewType), userId, -1);

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
            var uow = _fileUowProvider.GetUnitOfWork();

            IPartialViewRepository repository;
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    repository = _repositoryFactory.CreatePartialViewRepository(uow);
                    break;
                case PartialViewType.PartialViewMacro:
                default:
                    repository = _repositoryFactory.CreatePartialViewMacroRepository(uow);
                    break;
            }

            using (repository)
            {
                var partialView = repository.Get(path);
                if (partialView == null)
                    return true;

                if (DeletingPartialView.IsRaisedEventCancelled(new DeleteEventArgs<IPartialView>(partialView), this))
                    return false;

                repository.Delete(partialView);
                uow.Commit();

                DeletedPartialView.RaiseEvent(new DeleteEventArgs<IPartialView>(partialView, false), this);

                Audit(AuditType.Delete, string.Format("Delete {0} performed by user", partialViewType), userId, -1);
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
            if (SavingPartialView.IsRaisedEventCancelled(new SaveEventArgs<IPartialView>(partialView), this))
                return Attempt<IPartialView>.Fail();

            var uow = _fileUowProvider.GetUnitOfWork();

            IPartialViewRepository repository;
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    repository = _repositoryFactory.CreatePartialViewRepository(uow);
                    break;
                case PartialViewType.PartialViewMacro:
                default:
                    repository = _repositoryFactory.CreatePartialViewMacroRepository(uow);
                    break;
            }

            using (repository)
            {
                repository.AddOrUpdate(partialView);
                uow.Commit();

                SavedPartialView.RaiseEvent(new SaveEventArgs<IPartialView>(partialView, false), this);
            }

            Audit(AuditType.Save, string.Format("Save {0} performed by user", partialViewType), userId, -1);

            SavedPartialView.RaiseEvent(new SaveEventArgs<IPartialView>(partialView), this);

            return Attempt.Succeed(partialView);
        }

        public bool ValidatePartialView(PartialView partialView)
        {
            var validatePath = IOHelper.ValidateEditPath(partialView.Path, new[] { SystemDirectories.MvcViews + "/Partials/", SystemDirectories.MvcViews + "/MacroPartials/" });
            var verifyFileExtension = IOHelper.VerifyFileExtension(partialView.Path, new List<string> { "cshtml" });

            return validatePath && verifyFileExtension;
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

        private enum PartialViewType
        {
            PartialView,
            PartialViewMacro
        }

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            var uow = _dataUowProvider.GetUnitOfWork();
            using (var auditRepo = _repositoryFactory.CreateAuditRepository(uow))
            {
                auditRepo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Commit();
            }
        }

        #endregion

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