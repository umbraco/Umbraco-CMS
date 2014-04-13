using System;
using System.Collections.Generic;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
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
        /// <param name="name">Name incl. extension of the Stylesheet to delete</param>
        /// <param name="userId"></param>
        public void DeleteStylesheet(string name, int userId = 0)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateStylesheetRepository(uow, _dataUowProvider.GetUnitOfWork()))
            {
                var stylesheet = repository.Get(name);

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
        /// <param name="name">Name incl. extension of the Script to delete</param>
        /// <param name="userId"></param>
        public void DeleteScript(string name, int userId = 0)
        {
            var uow = _fileUowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateScriptRepository(uow))
            {
                var script = repository.Get(name);

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

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(params string[] aliases)
        {
            using (var repository = _repositoryFactory.CreateTemplateRepository(_dataUowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(aliases);
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

        #endregion
    }
}