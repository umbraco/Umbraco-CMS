using System;
using Umbraco.Core;
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

        public ServiceContextManager(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName;

            _application = StandaloneApplication.GetApplication();
            _application.Start();
        }

        public ServiceContext Services
        {
            get
            {
                if (_serviceContext == null)
                {
                    var dbFactory = new DefaultDatabaseFactory(_connectionString, _providerName);
                    var dbContext = new DatabaseContext(dbFactory);
                    Database.Mapper = new PetaPocoMapper();
                    _serviceContext = new ServiceContext(
                        new PetaPocoUnitOfWorkProvider(dbFactory),
                        new FileUnitOfWorkProvider(),
                        new PublishingStrategy());

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