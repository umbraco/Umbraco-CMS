using System.Collections.Generic;
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
        private readonly IUnitOfWork _fileUnitOfWork;
        private readonly IUnitOfWork _dataUnitOfWork;

        public FileService() : this(new FileUnitOfWorkProvider(), new PetaPocoUnitOfWorkProvider())
        {
        }

        public FileService(IUnitOfWorkProvider fileProvider, IUnitOfWorkProvider dataProvider)
        {
            _fileUnitOfWork = fileProvider.GetUnitOfWork();
            _dataUnitOfWork = dataProvider.GetUnitOfWork();
        }

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        public IEnumerable<Stylesheet> GetStylesheets(params string[] names)
        {
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(_fileUnitOfWork);
            return repository.GetAll(names);
        }

        /// <summary>
        /// Gets a <see cref="Stylesheet"/> object by its name
        /// </summary>
        /// <param name="name">Name of the stylesheet incl. extension</param>
        /// <returns>A <see cref="Stylesheet"/> object</returns>
        public Stylesheet GetStylesheetByName(string name)
        {
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(_fileUnitOfWork);
            return repository.Get(name);
        }

        /// <summary>
        /// Saves a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to save</param>
        public void SaveStylesheet(Stylesheet stylesheet)
        {
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(_fileUnitOfWork);
            repository.AddOrUpdate(stylesheet);
            _fileUnitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="name">Name incl. extension of the Stylesheet to delete</param>
        public void DeleteStylesheet(string name)
        {
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(_fileUnitOfWork);
            var stylesheet = repository.Get(name);
            repository.Delete(stylesheet);
            _fileUnitOfWork.Commit();
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
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(_fileUnitOfWork);
            return repository.GetAll(names);
        }

        /// <summary>
        /// Gets a <see cref="Script"/> object by its name
        /// </summary>
        /// <param name="name">Name of the script incl. extension</param>
        /// <returns>A <see cref="Script"/> object</returns>
        public Script GetScriptByName(string name)
        {
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(_fileUnitOfWork);
            return repository.Get(name);
        }

        /// <summary>
        /// Saves a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to save</param>
        public void SaveScript(Script script)
        {
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(_fileUnitOfWork);
            repository.AddOrUpdate(script);
            _fileUnitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="name">Name incl. extension of the Script to delete</param>
        public void DeleteScript(string name)
        {
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(_fileUnitOfWork);
            var script = repository.Get(name);
            repository.Delete(script);
            _fileUnitOfWork.Commit();
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

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(params string[] aliases)
        {
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, ITemplate, int>(_dataUnitOfWork);
            return repository.GetAll(aliases);
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias of the template</param>
        /// <returns>A <see cref="Template"/> object</returns>
        public ITemplate GetTemplateByAlias(string alias)
        {
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, ITemplate, int>(_dataUnitOfWork);
            return repository.Get(alias);
        }

        /// <summary>
        /// Saves a <see cref="Template"/>
        /// </summary>
        /// <param name="template"><see cref="Template"/> to save</param>
        public void SaveTemplate(ITemplate template)
        {
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, ITemplate, int>(_dataUnitOfWork);
            repository.AddOrUpdate(template);
            _dataUnitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a template by its alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="ITemplate"/> to delete</param>
        public void DeleteTemplate(string alias)
        {
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, ITemplate, int>(_dataUnitOfWork);
            var template = repository.Get(alias);
            repository.Delete(template);
            _dataUnitOfWork.Commit();
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
    }
}