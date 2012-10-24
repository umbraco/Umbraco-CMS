using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Web.Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWorkProvider _provider;

        public FileService() : this(new PetaPocoUnitOfWorkProvider())
        {
        }

        public FileService(IUnitOfWorkProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        public IEnumerable<Stylesheet> GetStylesheets(params string[] names)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(unitOfWork);
            return repository.GetAll(names);
        }

        /// <summary>
        /// Gets a <see cref="Stylesheet"/> object by its name
        /// </summary>
        /// <param name="name">Name of the stylesheet incl. extension</param>
        /// <returns>A <see cref="Stylesheet"/> object</returns>
        public Stylesheet GetStylesheetByName(string name)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(unitOfWork);
            return repository.Get(name);
        }

        /// <summary>
        /// Saves a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to save</param>
        public void SaveStylesheet(Stylesheet stylesheet)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(unitOfWork);
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="name">Name incl. extension of the Stylesheet to delete</param>
        public void DeleteStylesheet(string name)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(unitOfWork);
            var stylesheet = repository.Get(name);
            repository.Delete(stylesheet);
            unitOfWork.Commit();
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
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(unitOfWork);
            return repository.GetAll(names);
        }

        /// <summary>
        /// Gets a <see cref="Script"/> object by its name
        /// </summary>
        /// <param name="name">Name of the script incl. extension</param>
        /// <returns>A <see cref="Script"/> object</returns>
        public Script GetScriptByName(string name)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(unitOfWork);
            return repository.Get(name);
        }

        /// <summary>
        /// Saves a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to save</param>
        public void SaveScript(Script script)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(unitOfWork);
            repository.AddOrUpdate(script);
            unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="name">Name incl. extension of the Script to delete</param>
        public void DeleteScript(string name)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(unitOfWork);
            var script = repository.Get(name);
            repository.Delete(script);
            unitOfWork.Commit();
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
        /// Gets a list of all <see cref="Template"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Template"/> objects</returns>
        public IEnumerable<Template> GetTemplates(params string[] aliases)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, Template, string>(unitOfWork);
            return repository.GetAll(aliases);
        }

        /// <summary>
        /// Gets a <see cref="Template"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias of the template</param>
        /// <returns>A <see cref="Template"/> object</returns>
        public Template GetTemplateByAlias(string alias)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, Template, string>(unitOfWork);
            return repository.Get(alias);
        }

        /// <summary>
        /// Saves a <see cref="Template"/>
        /// </summary>
        /// <param name="template"><see cref="Template"/> to save</param>
        public void SaveTemplate(Template template)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, Template, string>(unitOfWork);
            repository.AddOrUpdate(template);
            unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a template by its alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="Template"/> to delete</param>
        public void DeleteTemplate(string alias)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, Template, string>(unitOfWork);
            var template = repository.Get(alias);
            repository.Delete(template);
            unitOfWork.Commit();
        }

        /// <summary>
        /// Validates a <see cref="Template"/>
        /// </summary>
        /// <param name="template"><see cref="Template"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(Template template)
        {
            return template.IsValid();
        }
    }
}