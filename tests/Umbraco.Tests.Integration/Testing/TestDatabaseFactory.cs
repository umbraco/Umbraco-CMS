// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class TestDatabaseFactory
    {
        public static ITestDatabase Create(TestDatabaseSettings settings, string filesPath, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactoryProvider)
        {
            string connectionString = Environment.GetEnvironmentVariable("UmbracoIntegrationTestConnectionString");

            switch (settings.Engine)
            {
                case "sqlite":
                    return CreateSQLite(settings, loggerFactory, dbFactoryProvider);
                    // TODO: Handle others.
            }
            return string.IsNullOrEmpty(connectionString)
                ? CreateLocalDb(settings, filesPath, loggerFactory, dbFactoryProvider)
                : CreateSqlDeveloper(settings, loggerFactory, dbFactoryProvider, connectionString);
        }

        private static ITestDatabase CreateSQLite(TestDatabaseSettings settings, ILoggerFactory loggerFactory,
            TestUmbracoDatabaseFactoryProvider dbFactoryProvider)
        {
            Directory.CreateDirectory(settings.FilesPath);

            return new SQLiteTestDatabase(settings, dbFactoryProvider, loggerFactory);
        }

        private static ITestDatabase CreateLocalDb(TestDatabaseSettings settings, string filesPath, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactoryProvider)
        {
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            var localDb = new LocalDb();

            if (!localDb.IsAvailable)
            {
                throw new InvalidOperationException("LocalDB is not available.");
            }

            return new LocalDbTestDatabase(settings, loggerFactory, localDb, filesPath, dbFactoryProvider.Create());
        }

        private static ITestDatabase CreateSqlDeveloper(TestDatabaseSettings settings, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactoryProvider, string connectionString)
        {
            // NOTE: Example setup for Linux box.
            // $ export SA_PASSWORD=Foobar123!
            // $ export UmbracoIntegrationTestConnectionString="Server=localhost,1433;User Id=sa;Password=$SA_PASSWORD;"
            // $ docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=$SA_PASSWORD" -e 'MSSQL_PID=Developer' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("ENV: UmbracoIntegrationTestConnectionString is not set");
            }

            return new SqlDeveloperTestDatabase(settings, loggerFactory, dbFactoryProvider.Create(), connectionString);
        }
    }
}
