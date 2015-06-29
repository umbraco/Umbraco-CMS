using System;
using System.Diagnostics;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Umbraco.Web.Standalone
{
    //TODO: Why does this exist? there's the same thing in the Core proj

    internal class ServiceContextManager : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _providerName;
        private readonly ILogger _logger;
        private readonly ISqlSyntaxProvider _syntaxProvider;
        private ServiceContext _serviceContext;
        private readonly StandaloneApplication _application;

        public ServiceContextManager(string connectionString, string providerName, string baseDirectory, ILogger logger, ISqlSyntaxProvider syntaxProvider)
        {
            _connectionString = connectionString;
            _providerName = providerName;
            _logger = logger;
            _syntaxProvider = syntaxProvider;

            Trace.WriteLine("Current AppDomain: " + AppDomain.CurrentDomain.FriendlyName);
            Trace.WriteLine("Current AppDomain: " + AppDomain.CurrentDomain.BaseDirectory);

            _application = StandaloneApplication.GetApplication(baseDirectory);
            _application.Start();
        }

        public ServiceContext Services
        {
            get
            {
                if (_serviceContext == null)
                {
                    var cacheHelper = new CacheHelper(
                        //SD: Not sure if this is correct? Should we be using HttpRuntime.Cache here since this is for 'Web' ?
                        //  just not quite sure what this standalone stuff is.
                        new ObjectCacheRuntimeCacheProvider(),
                        new StaticCacheProvider(),
                        //SD: Not sure if this is correct? Should we be using request cache here since this is for 'Web' ?
                        //  just not quite sure what this standalone stuff is.
                        new NullCacheProvider());

                    var dbFactory = new DefaultDatabaseFactory(_connectionString, _providerName, _logger);
                    var dbContext = new DatabaseContext(dbFactory, _logger, _syntaxProvider, _providerName);
                    Database.Mapper = new PetaPocoMapper();
                    _serviceContext = new ServiceContext(
                        new RepositoryFactory(cacheHelper, _logger, dbContext.SqlSyntax, UmbracoConfig.For.UmbracoSettings()), 
                        new PetaPocoUnitOfWorkProvider(dbFactory),
                        new FileUnitOfWorkProvider(),
                        new PublishingStrategy(),
                        cacheHelper,
                        new DebugDiagnosticsLogger());

                    //initialize the DatabaseContext
                    dbContext.Initialize(_providerName);
                }

                return _serviceContext;
            }
        }

        public void Dispose()
        {
            ((IDisposable)ApplicationContext.Current).Dispose();
            _application.Dispose();
        }
    }
}