using System;
using System.Diagnostics;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Umbraco.Core.Standalone
{
    internal class ServiceContextManager : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _providerName;
        private readonly ILogger _logger;
        private readonly ISqlSyntaxProvider _syntaxProvider;
        private ServiceContext _serviceContext;
        private readonly StandaloneCoreApplication _application;

        public ServiceContextManager(string connectionString, string providerName, string baseDirectory, ILogger logger, ISqlSyntaxProvider syntaxProvider)
        {
            _connectionString = connectionString;
            _providerName = providerName;
            _logger = logger;
            _syntaxProvider = syntaxProvider;

            Trace.WriteLine("ServiceContextManager-Current AppDomain: " + AppDomain.CurrentDomain.FriendlyName);
            Trace.WriteLine("ServiceContextManager-Current AppDomain: " + AppDomain.CurrentDomain.BaseDirectory);

            //var webAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("umbraco"));
            //if (webAssembly != null && examineEventHandlerToRemove == null)
            //{
            //    var examineEventType = webAssembly.GetType("Umbraco.Web.Search.ExamineEvents");
            //    examineEventHandlerToRemove = examineEventType;
            //}

            _application = StandaloneCoreApplication.GetApplication(baseDirectory);

            var examineEventHandlerToRemove = Type.GetType("Umbraco.Web.Search.ExamineEvents, umbraco");
            if (examineEventHandlerToRemove != null)
                _application.WithoutApplicationEventHandler(examineEventHandlerToRemove);

            _application.Start();
        }

        public ServiceContext Services
        {
            get
            {
                if (_serviceContext == null)
                {
                    var cacheHelper = new CacheHelper(
                        new ObjectCacheRuntimeCacheProvider(),
                        new StaticCacheProvider(),
                        //we have no request based cache when running standalone
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