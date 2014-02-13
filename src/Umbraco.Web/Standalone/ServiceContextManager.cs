using System;
using System.Diagnostics;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Umbraco.Web.Standalone
{
    internal class ServiceContextManager : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _providerName;
        private ServiceContext _serviceContext;
        private readonly StandaloneApplication _application;

        public ServiceContextManager(string connectionString, string providerName, string baseDirectory)
        {
            _connectionString = connectionString;
            _providerName = providerName;

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

                    var dbFactory = new DefaultDatabaseFactory(_connectionString, _providerName);
                    var dbContext = new DatabaseContext(dbFactory);
                    Database.Mapper = new PetaPocoMapper();
                    _serviceContext = new ServiceContext(
                        new PetaPocoUnitOfWorkProvider(dbFactory),
                        new FileUnitOfWorkProvider(),
                        new PublishingStrategy(),
                        cacheHelper);

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