using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Configuration.InfrastructureSettings;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    internal class RepositoryResolver
    {
        private static readonly ConcurrentDictionary<string, object> Repositories = new ConcurrentDictionary<string, object>();

        //GetRepository based on Repository Interface and Model Entity.
        //Check if Dictionary contains the interface name of the Repository, ea. IContentRepository
        //- If it does return the repository from the dictionary and set the new UnitOfWork object
        //Otherwise look for the full type for the repository in config
        //- If type exists check depedencies, create new object, add it to dictionary and return it
        //Otherwise look for an entity type in the config
        //- If type exists check dependencies, create new object, add it to dictionary and return it
        //If we have come this far the correct types wasn't found and we throw an exception
        internal static TRepository ResolveByType<TRepository, TEntity, TId>(IUnitOfWork unitOfWork)
            where TRepository : class, IRepository<TId, TEntity>
            where TEntity : class
        {
            //Initialize the provider's default value
            TRepository repository = default(TRepository);

            string interfaceShortName = typeof(TRepository).Name;
            string entityTypeName = typeof(TEntity).Name;

            //Check if the repository has already been created and is in the cache
            if (Repositories.ContainsKey(interfaceShortName))
            {
                repository = (TRepository)Repositories[interfaceShortName];
                if (unitOfWork != null && repository.GetType().IsSubclassOf(typeof(IRepository<TId, TEntity>)))
                {
                    repository.SetUnitOfWork(unitOfWork);
                }
                return repository;
            }
            
            var settings = Infrastructure.Instance.Repositories;

            Type repositoryType = null;

            //Check if a valid interfaceShortName was passed in
            if (settings.Repository.ContainsKey(interfaceShortName))
            {
                repositoryType = Type.GetType(settings.Repository[interfaceShortName].RepositoryFullTypeName);
            }
            else
            {
                foreach (Repository element in settings.Repository)
                {
                    if (element.InterfaceShortTypeName.Contains(entityTypeName))
                    {
                        repositoryType = Type.GetType(settings.Repository[element.InterfaceShortTypeName].RepositoryFullTypeName);
                        break;
                    }
                }
            }

            //If the repository type is null we should stop and throw an exception
            if (repositoryType == null)
            {
                throw new Exception(string.Format("No repository matching the Repository interface '{0}' or Entity type '{1}' could be resolved",
                interfaceShortName, entityTypeName));
            }

            //Resolve the repository with its constructor dependencies
            repository = Resolve(repositoryType, unitOfWork, interfaceShortName) as TRepository;

            //Add the new repository instance to the cache
            Repositories.AddOrUpdate(interfaceShortName, repository, (x, y) => repository);

            return repository;
        }

        //Recursive create and dependency check
        private static object Resolve(Type repositoryType, IUnitOfWork unitOfWork, string interfaceShortName)
        {
            var constructors = repositoryType.GetConstructors();
            var constructor = constructors.LastOrDefault();
            if (constructor == null)
            {
                throw new Exception(string.Format("No public constructor was found on {0}", repositoryType.FullName));
            }

            var constructorArgs = new List<object>();
            var settings = Infrastructure.Instance.Repositories;

            var parameters = constructor.GetParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType.Name.Equals("IUnitOfWork"))
                {
                    constructorArgs.Add(unitOfWork);
                }
                else if (Repositories.ContainsKey(parameter.ParameterType.Name))
                {
                    var repo = Repositories[parameter.ParameterType.Name];
                    constructorArgs.Add(repo);
                }
                else if(parameter.ParameterType.Name.Equals(typeof(IRepositoryCacheProvider).Name))
                {
                    //Get the Type of the cache provider and activate the singleton instance
                    var cacheType = Type.GetType(settings.Repository[interfaceShortName].CacheProviderFullTypeName);
                    if (cacheType == null)
                        throw new Exception(string.Format("Unable to get the type of cache provider '{0}' from configuration while creating an instance of {1}",
                                                          settings.Repository[parameter.ParameterType.Name].
                                                              CacheProviderFullTypeName,
                                                              parameter.ParameterType.Name));

                    var cacheProvider = cacheType.InvokeMember("Current", BindingFlags.GetProperty, null, cacheType, new object[0]);
                    constructorArgs.Add(cacheProvider);
                }
                else
                {
                    if (settings.Repository.ContainsKey(parameter.ParameterType.Name))
                    {
                        //Get the Type of the repository and resolve the object
                        var repoType = Type.GetType(settings.Repository[parameter.ParameterType.Name].RepositoryFullTypeName);
                        var repo = Resolve(repoType, unitOfWork, parameter.ParameterType.Name);

                        // Add the new repository instance to the cache
                        Repositories.AddOrUpdate(parameter.ParameterType.Name, repo, (x, y) => repo);

                        //Add the new repository to the constructor
                        constructorArgs.Add(repo);
                    }
                    else
                    {
                        throw new Exception("Cannot create the Repository. There was one or more invalid repository configuration settings.");
                    }
                }
            }

            if (parameters.Count() != constructorArgs.Count)
                throw new Exception(
                    string.Format(
                        "The amount of expected parameters is different from the amount of constructed parameters, which is confusing! Repository of type '{0}' could not be created.",
                        repositoryType.FullName));

            var repositoryObj = Activator.CreateInstance(repositoryType, constructorArgs.ToArray());
            return repositoryObj;
        }

        /// <summary>
        /// Register all repositories by interating the configuration and adding the repositories to the internal cache (Dictionary)
        /// </summary>
        internal static void RegisterRepositories()
        {
            var settings = Infrastructure.Instance.Repositories;

            foreach (Repository element in settings.Repository)
            {
                if (Repositories.ContainsKey(element.InterfaceShortTypeName)) continue;

                var repositoryType = Type.GetType(settings.Repository[element.InterfaceShortTypeName].RepositoryFullTypeName);
                var repository = Resolve(repositoryType, null, element.InterfaceShortTypeName);

                //Add the new repository instance to the cache
                Repositories.AddOrUpdate(element.InterfaceShortTypeName, repository, (x, y) => repository);
            }
        }

        /// <summary>
        /// Returns the number of repositories that has been registered
        /// </summary>
        /// <returns></returns>
        internal static int RegisteredRepositories()
        {
            return Repositories.Count;
        }
    }
}