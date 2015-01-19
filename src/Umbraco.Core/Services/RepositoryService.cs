using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Base service class
    /// </summary>
    public abstract class RepositoryService : IService
    {
        protected ILogger Logger { get; private set; }
        protected RepositoryFactory RepositoryFactory { get; private set; }
        protected IDatabaseUnitOfWorkProvider UowProvider { get; private set; }

        protected RepositoryService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (repositoryFactory == null) throw new ArgumentNullException("repositoryFactory");
            if (logger == null) throw new ArgumentNullException("logger");
            Logger = logger;
            RepositoryFactory = repositoryFactory;
            UowProvider = provider;
        }
    }
}