using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class SQLiteTestDatabase : ITestDatabase
    {
        private readonly TestDatabaseSettings _settings;
        private readonly IUmbracoDatabaseFactory _dbFactory;
        private readonly ILoggerFactory _loggerFactory;
        private int counter = 0;
        public const string DatabaseName = "UmbracoTests";

        public SQLiteTestDatabase(TestDatabaseSettings settings, IUmbracoDatabaseFactory dbFactory, ILoggerFactory loggerFactory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _loggerFactory = loggerFactory;
        }

        public TestDbMeta AttachEmpty()
        {
            var name = $"{DatabaseName}-{counter++}.db";
            return new TestDbMeta(name, true, CreateConnectionString(name));
        }

        public TestDbMeta AttachSchema()
        {
            var name = $"{DatabaseName}-{counter++}.db";
            var meta = new TestDbMeta(name, true, CreateConnectionString(name));

            _dbFactory.Configure(meta.ConnectionString, Constants.DatabaseProviders.SQLite);

            using (var database = (UmbracoDatabase)_dbFactory.CreateDatabase())
            {
                database.LogCommands = true;

                using (NPoco.ITransaction transaction = database.GetTransaction())
                {
                    var schemaCreator = new DatabaseSchemaCreator(database, _loggerFactory.CreateLogger<DatabaseSchemaCreator>(), _loggerFactory, new UmbracoVersion(), Mock.Of<IEventAggregator>());
                    schemaCreator.InitializeDatabaseSchema();

                    transaction.Complete();
                }
            }

            return meta;
        }

        public void Detach(TestDbMeta id)
        {

        }

        private string CreateConnectionString(string name)
        {
            var path = Path.Combine(_settings.FilesPath, $"{name}");
            return $"Data Source={path}";
        }
    }
}
