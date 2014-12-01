using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.Auditing;
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

        public FileService()
            : this(new RepositoryFactory())
        { }

        public FileService(RepositoryFactory repositoryFactory)
            : this(new FileUnitOfWorkProvider(), new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        public FileService(IUnitOfWorkProvider fileProvider, IDatabaseUnitOfWorkProvider dataProvider, RepositoryFactory repositoryFactory)
        {
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

            Audit.Add(AuditTypes.Save, string.Format("Save Stylesheet performed by user"), userId, -1);
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

                Audit.Add(AuditTypes.Delete, string.Format("Delete Stylesheet performed by user"), userId, -1);
            }
        }

        /// <summary>
        /// Validates a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to validate</param>
        /// <returns>True if Stylesheet is valid, otherwise false</returns>
        public bool ValidateStylesheet(Stylesheet stylesheet)
        {
            return stylesheet.IsValid() && stylesheet.IsFileValidCss();
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

            Audit.Add(AuditTypes.Save, string.Format("Save Script performed by user"), userId, -1);
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

                Audit.Add(AuditTypes.Delete, string.Format("Delete Script performed by user"), userId, -1);
            }
        }

        /// <summary>
        /// Validates a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateScript(Script script)
        {
            return script.IsValid();
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

            Audit.Add(AuditTypes.Save, string.Format("Save Template performed by user"), userId, template.Id);
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

            Audit.Add(AuditTypes.Save, string.Format("Save Template performed by user"), userId, -1);
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

                Audit.Add(AuditTypes.Delete, string.Format("Delete Template performed by user"), userId, template.Id);
            }
        }

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(ITemplate template)
        {
            return template.IsValid();
        } 
        #endregion

        #region Partial Views

        internal IEnumerable<string> GetPartialViewSnippetNames(params string[] filterNames)
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

        internal void DeletePartialViewFolder(string folderPath)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewRepository(uow))
            {
                ((PartialViewRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        internal void DeletePartialViewMacroFolder(string folderPath)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            var duow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewMacroRepository(uow, duow))
            {
                ((PartialViewMacroRepository)repository).DeleteFolder(folderPath);
                uow.Commit();
            }
        }

        internal PartialView GetPartialView(string path)
        {
            using (var repository = _repositoryFactory.CreatePartialViewRepository(_fileUowProvider.GetUnitOfWork()))
            {
                return repository.Get(path);
            }
        }

        internal PartialView GetPartialViewMacro(string path)
        {
            using (var repository = _repositoryFactory.CreatePartialViewMacroRepository(
                _fileUowProvider.GetUnitOfWork(),
                _dataUowProvider.GetUnitOfWork()))
            {
                return repository.Get(path);
            }
        }

        internal Attempt<PartialView> CreatePartialView(PartialView partialView, string snippetName = null, int userId = 0)
        {
            if (CreatingPartialView.IsRaisedEventCancelled(new NewEventArgs<PartialView>(partialView, true, partialView.Alias, -1), this))
                return Attempt<PartialView>.Fail();

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

                    var content = string.Format("{0}{1}{2}", PartialViewHeader, Environment.NewLine, snippetContent);
                    partialView.Content = content;
                }
            }

            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewRepository(uow))
            {
                repository.AddOrUpdate(partialView);
                uow.Commit();

                CreatedPartialView.RaiseEvent(new NewEventArgs<PartialView>(partialView, false, partialView.Alias, -1), this);
            }

            Audit.Add(AuditTypes.Save, string.Format("Save PartialView performed by user"), userId, -1);

            return Attempt<PartialView>.Succeed(partialView);
        }

        internal Attempt<PartialView> CreatePartialViewMacro(PartialView partialView, bool createMacro, string snippetName = null, int userId = 0)
        {
            if (CreatingPartialView.IsRaisedEventCancelled(new NewEventArgs<PartialView>(partialView, true, partialView.Alias, -1), this))
                return Attempt<PartialView>.Fail();

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

                    var content = string.Format("{0}{1}{2}", PartialViewMacroHeader, Environment.NewLine, snippetContent);
                    partialView.Content = content;
                }
            }

            var uow = _fileUowProvider.GetUnitOfWork();
            var duow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewMacroRepository(uow, duow))
            {
                repository.AddOrUpdate(partialView);
                
                if (createMacro)
                {
                    var name = Path.GetFileNameWithoutExtension(partialView.Path)
                        .SplitPascalCasing()
                        .ToFirstUpperInvariant()
                        .ToSafeAlias(false);

                    //The partial view path to be saved with the macro must be a fully qualified virtual path
                    var virtualPath = string.Format("{0}/{1}/{2}", SystemDirectories.MvcViews, "MacroPartials", partialView.Path);

                    repository.AddOrUpdate(new Macro(name, name) { ScriptPath = virtualPath });
                }

                //commit both - ensure that the macro is created if one was added
                uow.Commit();
                duow.Commit();

                CreatedPartialView.RaiseEvent(new NewEventArgs<PartialView>(partialView, false, partialView.Alias, -1), this);
            }

            Audit.Add(AuditTypes.Save, string.Format("Save PartialViewMacro performed by user"), userId, -1);

            return Attempt<PartialView>.Succeed(partialView);           
        }

        internal bool DeletePartialView(string path, int userId = 0)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewRepository(uow))
            {
                var partialView = repository.Get(path);
                if (partialView == null)
                    return true;

                if (DeletingPartialView.IsRaisedEventCancelled(new DeleteEventArgs<PartialView>(partialView), this))
                    return false;

                repository.Delete(partialView);
                uow.Commit();

                DeletedPartialView.RaiseEvent(new DeleteEventArgs<PartialView>(partialView, false), this);

                Audit.Add(AuditTypes.Delete, string.Format("Delete PartialView performed by user"), userId, -1);
            }

            return true;

        }

        internal bool DeletePartialViewMacro(string path, int userId = 0)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            var duow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewMacroRepository(uow, duow))
            {
                var partialView = repository.Get(path);
                if (partialView == null)
                    return true;

                if (DeletingPartialView.IsRaisedEventCancelled(new DeleteEventArgs<PartialView>(partialView), this))
                    return false;

                repository.Delete(partialView);
                
                //commit both (though in the case of deleting, there's no db changes)
                uow.Commit();
                duow.Commit();

                DeletedPartialView.RaiseEvent(new DeleteEventArgs<PartialView>(partialView, false), this);

                Audit.Add(AuditTypes.Delete, string.Format("Delete PartialViewMacro performed by user"), userId, -1);
            }

            return true;

        }

        internal Attempt<PartialView> SavePartialView(PartialView partialView, int userId = 0)
        {
            if (SavingPartialView.IsRaisedEventCancelled(new SaveEventArgs<PartialView>(partialView), this))
                return Attempt<PartialView>.Fail();

            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewRepository(uow))
            {
                repository.AddOrUpdate(partialView);
                uow.Commit();

                SavedPartialView.RaiseEvent(new SaveEventArgs<PartialView>(partialView, false), this);
            }

            Audit.Add(AuditTypes.Save, string.Format("Save PartialView performed by user"), userId, -1);

            ////NOTE: I've left the below here just for informational purposes. If we save a file this way, then the UTF8
            //// BOM mucks everything up, strangely, if we use WriteAllText everything is ok! 
            //// http://issues.umbraco.org/issue/U4-2118
            ////using (var sw = System.IO.File.CreateText(savePath))
            ////{
            ////    sw.Write(val);
            ////}

            //System.IO.File.WriteAllText(partialView.Path, partialView.Content, Encoding.UTF8);
            ////deletes the old file
            //if (partialView.FileName != partialView.OldFileName)
            //{
            //    // Create a new PartialView class so that we can set the FileName of the file that needs deleting
            //    var deletePartial = partialView;
            //    deletePartial.FileName = partialView.OldFileName;
            //    DeletePartialView(deletePartial, userId);
            //}

            //SavedPartialView.RaiseEvent(new SaveEventArgs<PartialView>(partialView), this);

            return Attempt.Succeed(partialView);
        }

        internal Attempt<PartialView> SavePartialViewMacro(PartialView partialView, int userId = 0)
        {
            if (SavingPartialView.IsRaisedEventCancelled(new SaveEventArgs<PartialView>(partialView), this))
                return Attempt<PartialView>.Fail();
            
            var uow = _fileUowProvider.GetUnitOfWork();
            var duow = _dataUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreatePartialViewMacroRepository(uow, duow))
            {
                repository.AddOrUpdate(partialView);

                //commit both (though in the case of updating, there's no db changes)
                uow.Commit();
                duow.Commit();

                SavedPartialView.RaiseEvent(new SaveEventArgs<PartialView>(partialView, false), this);
            }

            Audit.Add(AuditTypes.Save, string.Format("Save PartialViewMacro performed by user"), userId, -1);

            ////NOTE: I've left the below here just for informational purposes. If we save a file this way, then the UTF8
            //// BOM mucks everything up, strangely, if we use WriteAllText everything is ok! 
            //// http://issues.umbraco.org/issue/U4-2118
            ////using (var sw = System.IO.File.CreateText(savePath))
            ////{
            ////    sw.Write(val);
            ////}

            //System.IO.File.WriteAllText(partialView.Path, partialView.Content, Encoding.UTF8);
            ////deletes the old file
            //if (partialView.FileName != partialView.OldFileName)
            //{
            //    // Create a new PartialView class so that we can set the FileName of the file that needs deleting
            //    var deletePartial = partialView;
            //    deletePartial.FileName = partialView.OldFileName;
            //    DeletePartialView(deletePartial, userId);
            //}

            //SavedPartialView.RaiseEvent(new SaveEventArgs<PartialView>(partialView), this);

            return Attempt.Succeed(partialView);
        }

        internal bool ValidatePartialView(PartialView partialView)
        {
            return partialView.IsValid();
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
        internal static event TypedEventHandler<IFileService, SaveEventArgs<PartialView>> SavingPartialView;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        internal static event TypedEventHandler<IFileService, SaveEventArgs<PartialView>> SavedPartialView;
        
        /// <summary>
        /// Occurs before Create
        /// </summary>
        internal static event TypedEventHandler<IFileService, NewEventArgs<PartialView>> CreatingPartialView;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        internal static event TypedEventHandler<IFileService, NewEventArgs<PartialView>> CreatedPartialView;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        internal static event TypedEventHandler<IFileService, DeleteEventArgs<PartialView>> DeletingPartialView;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        internal static event TypedEventHandler<IFileService, DeleteEventArgs<PartialView>> DeletedPartialView;

        #endregion



        
    }
}