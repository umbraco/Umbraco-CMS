// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public static class TestDatabaseFactory
    {
        public static ITestDatabase Create(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactory, ILoggerFactory loggerFactory) =>
            settings.Provider switch
            {
                Persistence.Sqlite.Constants.ProviderName => new SqliteTestDatabase(settings, dbFactory, loggerFactory),
                Core.Constants.DatabaseProviders.SqlServer => CreateSqlServer(settings, dbFactory, loggerFactory),
                _ => throw new ApplicationException("Unsupported test database provider")
            };

        private static ITestDatabase CreateSqlServer(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactory, ILoggerFactory loggerFactory)
        {
            return string.IsNullOrEmpty(settings.SQLServerMasterConnectionString)
                ? CreateLocalDb(settings, loggerFactory, dbFactory)
                : CreateSqlDeveloper(settings, loggerFactory, dbFactory);
        }

        private static ITestDatabase CreateLocalDb(TestDatabaseSettings settings, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
        {
            var localDb = new LocalDb();

            if (!localDb.IsAvailable)
            {
                throw new InvalidOperationException("LocalDB is not available.");
            }

            return new LocalDbTestDatabase(settings, loggerFactory, localDb,  dbFactory.Create());
        }

        private static ITestDatabase CreateSqlDeveloper(TestDatabaseSettings settings, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
        {
            // NOTE: Example setup for Linux box.
            // $ export SA_PASSWORD=Foobar123!
            // $ docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=$SA_PASSWORD" -e 'MSSQL_PID=Developer' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu
     
            return new SqlServerTestDatabase(settings, loggerFactory, dbFactory.Create());
        }
    }
}
